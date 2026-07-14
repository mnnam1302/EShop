using EShop.Shared.Diagnostics;
using EShop.Shared.RateLimiting.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Threading.RateLimiting;

namespace EShop.Shared.RateLimiting.AspNetCore;

public sealed class DistributedSlidingWindowRateLimiter : RateLimiter
{
    private readonly IRateLimiter _rateLimiter;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Func<SlidingWindowCheck> _checkFactory;
    private readonly string _scopeName;
    private readonly string _tenantId;
    private readonly string _domain;
    private readonly IOptionsMonitor<RateLimiterEnforcementOptions> _enforcementOptions;

    public DistributedSlidingWindowRateLimiter(
        IRateLimiter rateLimiter,
        IHttpContextAccessor httpContextAccessor,
        Func<SlidingWindowCheck> checkFactory,
        string scopeName,
        string tenantId,
        string domain,
        IOptionsMonitor<RateLimiterEnforcementOptions> enforcementOptions)
    {
        _rateLimiter = rateLimiter;
        _httpContextAccessor = httpContextAccessor;
        _checkFactory = checkFactory;
        _scopeName = scopeName;
        _tenantId = tenantId;
        _domain = domain;
        _enforcementOptions = enforcementOptions;
    }

    public override TimeSpan? IdleDuration => null;

    protected override async ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken)
    {
        var check = _checkFactory();
        var result = await _rateLimiter.CheckSlidingWindowAsync(check, cancellationToken);

        RateLimitHeaderWriter.Write(_httpContextAccessor.HttpContext, check.Limit, result.Remaining, (long)check.Window.TotalSeconds);

        var enforced = _enforcementOptions.CurrentValue.IsEnforced(_scopeName);
        var decision = RateLimitDecisionNames.From(result.Allowed, enforced);
        RateLimiterMetrics.Requests.Add(1, new TagList
        {
            { "decision", decision },
            { "layer", _scopeName },
            { "tenant", _tenantId },
            { "domain", _domain }
        });

        if (!result.Allowed && enforced)
        {
            return new DistributedRateLimitLease(false, result.Remaining, result.RetryAfter, _scopeName);
        }

        return new DistributedRateLimitLease(true, result.Remaining, TimeSpan.Zero, exceededScope: null);
    }

    protected override RateLimitLease AttemptAcquireCore(int permitCount)
    {
        return new DistributedRateLimitLease(isAcquired: false, remaining: 0, retryAfter: TimeSpan.Zero, exceededScope: null);
    }

    public override RateLimiterStatistics? GetStatistics() => null;
}
