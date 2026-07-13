namespace EShop.Shared.RateLimiting;

public sealed class RateLimiterOptions
{
    public TimeSpan RedisTimeout { get; init; } = TimeSpan.FromMilliseconds(50);
}
