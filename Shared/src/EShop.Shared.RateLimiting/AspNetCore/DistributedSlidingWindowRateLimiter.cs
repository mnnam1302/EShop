using EShop.Shared.RateLimiting.Abstractions;
using System.Threading.RateLimiting;

namespace EShop.Shared.RateLimiting.AspNetCore;

public sealed class DistributedSlidingWindowRateLimiter : RateLimiter
{
    private readonly IRateLimiter _rateLimiter;
    private readonly SlidingWindowCheck _check;
    private readonly string _scopeName;

    public DistributedSlidingWindowRateLimiter(IRateLimiter rateLimiter, SlidingWindowCheck check, string scopeName)
    {
        _rateLimiter = rateLimiter;
        _check = check;
        _scopeName = scopeName;
    }

    public override TimeSpan? IdleDuration => null;

    protected override async ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken)
    {
        var result = await _rateLimiter.CheckSlidingWindowAsync(_check, cancellationToken);

        var exceededScope = result.Allowed ? null : _scopeName;
        return new DistributedRateLimitLease(result.Allowed, result.Remaining, result.RetryAfter, exceededScope);
    }

    protected override RateLimitLease AttemptAcquireCore(int permitCount)
    {
        return new DistributedRateLimitLease(isAcquired: false, remaining: 0, retryAfter: TimeSpan.Zero, exceededScope: null);
    }

    public override RateLimiterStatistics? GetStatistics() => null;
}
