namespace EShop.Shared.RateLimiting;

public interface IRateLimiter
{
    Task<CombinedRateLimitResult> CheckTokenBucketsAsync(
        TokenBucketCheck primary,
        TokenBucketCheck? secondary,
        CancellationToken cancellationToken = default);

    Task<RateLimitResult> CheckSlidingWindowAsync(
        SlidingWindowCheck check,
        CancellationToken cancellationToken = default);
}
