using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.RateLimiting;

public static class RateLimitingServiceCollectionExtensions
{
    public static IServiceCollection AddDistributedRateLimiter(this IServiceCollection services)
    {
        services.AddSingleton<IRateLimiter, RedisRateLimiter>();

        return services;
    }
}
