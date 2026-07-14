using EShop.Shared.RateLimiting.Abstractions;
using EShop.Shared.RateLimiting.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShop.Shared.RateLimiting.DependencyInjections;

public static class RateLimitingServiceCollectionExtensions
{
    public static IServiceCollection AddDistributedRateLimiter(this IServiceCollection services)
    {
        // FailOpenRateLimiter wraps RedisRateLimiter with the resilience/fail-open behavior (D10).
        // Both are registered as singletons — RedisRateLimiter loads its Lua scripts once at
        // construction, and FailOpenRateLimiter's only dependencies (IRateLimiter, ILogger<T>) are
        // themselves singleton-safe, so nothing here captures a scoped service into a singleton.
        services.AddSingleton<RedisRateLimiter>();
        services.AddSingleton<IRateLimiter>(sp => new FailOpenRateLimiter(
            sp.GetRequiredService<RedisRateLimiter>(),
            sp.GetRequiredService<ILogger<FailOpenRateLimiter>>()));

        return services;
    }
}
