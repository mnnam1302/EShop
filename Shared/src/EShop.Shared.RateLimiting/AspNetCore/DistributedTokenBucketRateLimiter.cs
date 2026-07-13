using EShop.Shared.RateLimiting.Abstractions;
using System.Threading.RateLimiting;

namespace EShop.Shared.RateLimiting.AspNetCore;

public sealed class DistributedTokenBucketRateLimiter : RateLimiter
{
    private readonly IRateLimiter _rateLimiter;
    private readonly TokenBucketCheck _primary;
    private readonly string _primaryScopeName;
    private readonly TokenBucketCheck? _secondary;
    private readonly string? _secondaryScopeName;

    public DistributedTokenBucketRateLimiter(
        IRateLimiter rateLimiter,
        TokenBucketCheck primary,
        string primaryScopeName,
        TokenBucketCheck? secondary = null,
        string? secondaryScopeName = null)
    {
        _rateLimiter = rateLimiter;
        _primary = primary;
        _primaryScopeName = primaryScopeName;
        _secondary = secondary;
        _secondaryScopeName = secondaryScopeName;
    }

    public override TimeSpan? IdleDuration => null;

    protected override async ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken)
    {
        var result = await _rateLimiter.CheckTokenBucketsAsync(_primary, _secondary, cancellationToken);

        if (!result.Primary.Allowed)
        {
            return new DistributedRateLimitLease(false, result.Primary.Remaining, result.Primary.RetryAfter, _primaryScopeName);
        }

        if (result.Secondary is { Allowed: false } rejectedSecondary)
        {
            return new DistributedRateLimitLease(false, rejectedSecondary.Remaining, rejectedSecondary.RetryAfter, _secondaryScopeName);
        }

        return new DistributedRateLimitLease(true, result.Primary.Remaining, TimeSpan.Zero, exceededScope: null);
    }

    protected override RateLimitLease AttemptAcquireCore(int permitCount)
    {
        return new DistributedRateLimitLease(isAcquired: false, remaining: 0, retryAfter: TimeSpan.Zero, exceededScope: null);
    }

    public override RateLimiterStatistics? GetStatistics() => null;
}
