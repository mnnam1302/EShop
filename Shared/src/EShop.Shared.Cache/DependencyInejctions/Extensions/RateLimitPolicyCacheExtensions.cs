using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.Cache.DependencyInejctions.Extensions;

public static class RateLimitPolicyCacheExtensions
{
    public static IServiceCollection AddRateLimitPolicyCachingService(this IServiceCollection services)
    {
        services.AddScoped<IRateLimitPolicyCachingService, RateLimitPolicyCachingService>();
        services.AddScoped<IRedisCachingProvider<CachedRateLimitPolicy>, RedisCachingProvider<CachedRateLimitPolicy>>();

        return services;
    }
}
