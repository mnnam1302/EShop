namespace EShop.Shared.RateLimiting.Abstractions;

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
