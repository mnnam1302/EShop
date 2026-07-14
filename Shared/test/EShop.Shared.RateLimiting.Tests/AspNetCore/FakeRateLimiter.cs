using EShop.Shared.RateLimiting.Abstractions;

namespace EShop.Shared.RateLimiting.Tests.AspNetCore;

// A stub IRateLimiter with pre-programmed results. Shadow/enforce decision logic lives entirely in
// DistributedTokenBucketRateLimiter/DistributedSlidingWindowRateLimiter's interpretation of the
// result they receive — it doesn't depend on Redis, so these tests don't need Testcontainers.
internal sealed class FakeRateLimiter : IRateLimiter
{
    public CombinedRateLimitResult TokenBucketResult { get; set; } = new(new RateLimitResult(true, 1, TimeSpan.Zero), null);

    public RateLimitResult SlidingWindowResult { get; set; } = new(true, 1, TimeSpan.Zero);

    public Task<CombinedRateLimitResult> CheckTokenBucketsAsync(TokenBucketCheck primary, TokenBucketCheck? secondary, CancellationToken cancellationToken = default) =>
        Task.FromResult(TokenBucketResult);

    public Task<RateLimitResult> CheckSlidingWindowAsync(SlidingWindowCheck check, CancellationToken cancellationToken = default) =>
        Task.FromResult(SlidingWindowResult);
}
