using EShop.Shared.RateLimiting.Abstractions;
using System.Collections.Concurrent;
using System.Threading.RateLimiting;

namespace EShop.Shared.RateLimiting.Redis;

// Layer-0 fallback (design D10): a per-node, in-memory limiter used only while FailOpenRateLimiter's
// circuit to Redis is open. Built on the BCL's own token-bucket/sliding-window limiters rather than
// reimplementing bucket math — this path exists purely so a Redis outage degrades to "bounded
// per-node admission" instead of either an unhandled exception or an unbounded flood.
internal sealed class InMemoryFallbackRateLimiter
{
    private readonly ConcurrentDictionary<string, RateLimiter> _limiters = new();

    public async Task<CombinedRateLimitResult> CheckTokenBucketsAsync(
        TokenBucketCheck primary,
        TokenBucketCheck? secondary,
        CancellationToken cancellationToken)
    {
        var primaryResult = await CheckTokenBucketAsync(primary, cancellationToken);
        var secondaryResult = secondary is null ? null : await CheckTokenBucketAsync(secondary, cancellationToken);

        return new CombinedRateLimitResult(primaryResult, secondaryResult);
    }

    public async Task<RateLimitResult> CheckSlidingWindowAsync(SlidingWindowCheck check, CancellationToken cancellationToken)
    {
        var limiter = (SlidingWindowRateLimiter)_limiters.GetOrAdd(check.Key, _ => new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
        {
            PermitLimit = check.Limit,
            Window = check.Window,
            SegmentsPerWindow = 2,
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true
        }));

        using var lease = await limiter.AcquireAsync(1, cancellationToken);
        return ToResult(limiter, lease);
    }

    private async Task<RateLimitResult> CheckTokenBucketAsync(TokenBucketCheck check, CancellationToken cancellationToken)
    {
        var limiter = (TokenBucketRateLimiter)_limiters.GetOrAdd(check.Key, _ => new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = check.Capacity,
            TokensPerPeriod = check.RefillTokensPerPeriod,
            ReplenishmentPeriod = check.RefillPeriod,
            AutoReplenishment = true,
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        }));

        using var lease = await limiter.AcquireAsync(1, cancellationToken);
        return ToResult(limiter, lease);
    }

    private static RateLimitResult ToResult(RateLimiter limiter, RateLimitLease lease)
    {
        var remaining = limiter.GetStatistics()?.CurrentAvailablePermits ?? 0;

        var retryAfter = TimeSpan.Zero;
        if (!lease.IsAcquired && lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue))
        {
            retryAfter = retryAfterValue;
        }

        return new RateLimitResult(lease.IsAcquired, remaining, retryAfter);
    }
}
