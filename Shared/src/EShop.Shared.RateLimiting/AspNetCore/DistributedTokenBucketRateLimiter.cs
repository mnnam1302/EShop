using EShop.Shared.Diagnostics;
using EShop.Shared.RateLimiting.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Threading.RateLimiting;

namespace EShop.Shared.RateLimiting.AspNetCore;

public sealed class DistributedTokenBucketRateLimiter : RateLimiter
{
    private readonly IRateLimiter _rateLimiter;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Func<TokenBucketCheck> _primaryFactory;
    private readonly string _primaryScopeName;
    private readonly string _tenantId;
    private readonly string _domain;
    private readonly IOptionsMonitor<RateLimiterEnforcementOptions> _enforcementOptions;
    private readonly Func<TokenBucketCheck>? _secondaryFactory;
    private readonly string? _secondaryScopeName;

    // The partition wrapper this class implements is reused by ASP.NET Core across every request
    // that maps to the same partition key, but the rate-limit rule behind it can change at runtime
    // (tenant quota edits, plan changes). Taking factories instead of frozen TokenBucketCheck values
    // means each acquire re-resolves the current rule (a cheap in-process cache read) instead of
    // permanently applying whatever rule was in effect the first time this partition was created.
    // enforcementOptions is read the same way (IOptionsMonitor.CurrentValue, not a snapshot) so a
    // layer's shadow/enforce flag can flip via config reload without recreating this instance either.
    //
    // tenantId/domain, unlike the rule factories, are safe to capture once: they're baked into the
    // partition key itself (RateLimitingExtensions partitions by "{tenantId}:{userId}:{domain}"), so
    // a given instance's tenantId/domain never change across the requests it's reused for.
    public DistributedTokenBucketRateLimiter(
        IRateLimiter rateLimiter,
        IHttpContextAccessor httpContextAccessor,
        Func<TokenBucketCheck> primaryFactory,
        string primaryScopeName,
        string tenantId,
        string domain,
        IOptionsMonitor<RateLimiterEnforcementOptions> enforcementOptions,
        Func<TokenBucketCheck>? secondaryFactory = null,
        string? secondaryScopeName = null)
    {
        _rateLimiter = rateLimiter;
        _httpContextAccessor = httpContextAccessor;
        _primaryFactory = primaryFactory;
        _primaryScopeName = primaryScopeName;
        _tenantId = tenantId;
        _domain = domain;
        _enforcementOptions = enforcementOptions;
        _secondaryFactory = secondaryFactory;
        _secondaryScopeName = secondaryScopeName;
    }

    public override TimeSpan? IdleDuration => null;

    protected override async ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken)
    {
        var primary = _primaryFactory();
        var secondary = _secondaryFactory?.Invoke();

        var result = await _rateLimiter.CheckTokenBucketsAsync(primary, secondary, cancellationToken);

        // The RateLimit-* headers are informational client hints, written here (before next() /
        // OnRejected run) because AcquireAsyncCore has no other hook into the response — .NET's rate
        // limiting middleware doesn't expose a "request admitted" callback the way it exposes OnRejected.
        // IHttpContextAccessor is safe to use from an instance reused across requests/partitions
        // because it resolves the CURRENT ambient HttpContext via AsyncLocal on every call, not a
        // value captured once at construction.
        var primaryEnforced = _enforcementOptions.CurrentValue.IsEnforced(_primaryScopeName);
        RecordDecision(_primaryScopeName, result.Primary.Allowed, primaryEnforced);

        if (!result.Primary.Allowed && primaryEnforced)
        {
            WriteHeaders(primary, result.Primary);
            return new DistributedRateLimitLease(false, result.Primary.Remaining, result.Primary.RetryAfter, _primaryScopeName);
        }

        if (result.Secondary is { } secondaryResult && _secondaryScopeName is not null)
        {
            var secondaryEnforced = _enforcementOptions.CurrentValue.IsEnforced(_secondaryScopeName);
            RecordDecision(_secondaryScopeName, secondaryResult.Allowed, secondaryEnforced);

            if (!secondaryResult.Allowed && secondaryEnforced)
            {
                WriteHeaders(secondary!, secondaryResult);
                return new DistributedRateLimitLease(false, secondaryResult.Remaining, secondaryResult.RetryAfter, _secondaryScopeName);
            }
        }

        var (mostRestrictiveCheck, mostRestrictiveResult) = PickMostRestrictive(primary, result.Primary, secondary, result.Secondary);
        WriteHeaders(mostRestrictiveCheck, mostRestrictiveResult);

        return new DistributedRateLimitLease(true, result.Primary.Remaining, TimeSpan.Zero, exceededScope: null);
    }

    // decision is "allow" when the check itself passed; otherwise "reject" if this layer is enforced,
    // or "shadow_reject" if the layer is still in shadow mode (would have rejected, admitted anyway).
    private void RecordDecision(string layer, bool allowed, bool enforced)
    {
        var decision = RateLimitDecisionNames.From(allowed, enforced);
        RateLimiterMetrics.Requests.Add(1, new TagList
        {
            { "decision", decision },
            { "layer", layer },
            { "tenant", _tenantId },
            { "domain", _domain }
        });
    }

    protected override RateLimitLease AttemptAcquireCore(int permitCount)
    {
        return new DistributedRateLimitLease(isAcquired: false, remaining: 0, retryAfter: TimeSpan.Zero, exceededScope: null);
    }

    public override RateLimiterStatistics? GetStatistics() => null;

    private void WriteHeaders(TokenBucketCheck check, RateLimitResult result)
    {
        var resetSeconds = ComputeResetSeconds(check, result.Remaining);
        RateLimitHeaderWriter.Write(_httpContextAccessor.HttpContext, check.Capacity, result.Remaining, resetSeconds);
    }

    private static long ComputeResetSeconds(TokenBucketCheck check, long remaining)
    {
        var refillRatePerSecond = check.RefillTokensPerPeriod / check.RefillPeriod.TotalSeconds;
        if (refillRatePerSecond <= 0)
        {
            return 0;
        }

        var tokensNeeded = Math.Max(0, check.Capacity - remaining);
        return (long)Math.Ceiling(tokensNeeded / refillRatePerSecond);
    }

    private static (TokenBucketCheck Check, RateLimitResult Result) PickMostRestrictive(
        TokenBucketCheck primaryCheck,
        RateLimitResult primaryResult,
        TokenBucketCheck? secondaryCheck,
        RateLimitResult? secondaryResult)
    {
        if (secondaryCheck is null || secondaryResult is null)
        {
            return (primaryCheck, primaryResult);
        }

        var primaryRatio = primaryCheck.Capacity == 0 ? 0 : (double)primaryResult.Remaining / primaryCheck.Capacity;
        var secondaryRatio = secondaryCheck.Capacity == 0 ? 0 : (double)secondaryResult.Remaining / secondaryCheck.Capacity;

        return secondaryRatio < primaryRatio ? (secondaryCheck, secondaryResult) : (primaryCheck, primaryResult);
    }
}
