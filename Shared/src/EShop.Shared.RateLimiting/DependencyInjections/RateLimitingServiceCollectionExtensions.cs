using EShop.Shared.RateLimiting.Abstractions;
using EShop.Shared.RateLimiting.Redis;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.RateLimiting.DependencyInjections;

public static class RateLimitingServiceCollectionExtensions
{
    public static IServiceCollection AddDistributedRateLimiter(this IServiceCollection services)
    {
        services.AddSingleton<IRateLimiter, RedisRateLimiter>();

        return services;
    }
}
