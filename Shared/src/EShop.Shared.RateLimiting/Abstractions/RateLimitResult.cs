namespace EShop.Shared.RateLimiting.Abstractions;

public sealed record RateLimitResult(bool Allowed, long Remaining, TimeSpan RetryAfter);

public sealed record CombinedRateLimitResult(RateLimitResult Primary, RateLimitResult? Secondary)
{
    public bool Allowed => Primary.Allowed && (Secondary?.Allowed ?? true);
}
