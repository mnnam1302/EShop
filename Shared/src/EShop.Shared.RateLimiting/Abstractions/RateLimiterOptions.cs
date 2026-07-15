namespace EShop.Shared.RateLimiting.Abstractions;

public sealed class RateLimiterOptions
{
    public TimeSpan RedisTimeout { get; init; } = TimeSpan.FromMilliseconds(50);
}
