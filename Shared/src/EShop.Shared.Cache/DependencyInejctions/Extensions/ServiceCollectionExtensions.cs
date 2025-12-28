using EShop.Shared.Cache.DependencyInejctions.Options;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EShop.Shared.Cache.DependencyInejctions.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedisHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        var redisOptions = new RedisOptions();
        configuration.GetSection(nameof(RedisOptions)).Bind(redisOptions);

        if (!redisOptions.Enabled)
        {
            return services;
        }

        // Check if running with .NET Aspire (connection name from service discovery)
        var aspireRedisConnectionString = configuration.GetConnectionString("redis");
        var connectionString = !string.IsNullOrEmpty(aspireRedisConnectionString) 
            ? aspireRedisConnectionString 
            : redisOptions.ConnectionString;

        services
            .AddHealthChecks()
            .AddRedis(connectionString, "redis", HealthStatus.Degraded, ["cache", "redis"]);

        return services;
    }

    public static IServiceCollection AddRedisCacheInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var redisOptions = new RedisOptions();
        configuration.GetSection(nameof(RedisOptions)).Bind(redisOptions);
        services.AddSingleton(redisOptions);

        if (!redisOptions.Enabled)
            return services;

        // Check if running with .NET Aspire (connection name from service discovery)
        var aspireRedisConnectionString = configuration.GetConnectionString("redis");
        if (!string.IsNullOrEmpty(aspireRedisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = aspireRedisConnectionString;
            });
        }
        else
        {
            // Fallback to manual configuration
            services.AddStackExchangeRedisCache(options =>
            {
                var connectionString = redisOptions.ConnectionString;
                options.Configuration = connectionString;
            });
        }

        services.AddScoped(typeof(CachedRemoteConfiguration));
        services.AddScoped<IRedisResiliencePolicyProvider, RedisResiliencePolicyProvider>();

        return services;
    }

    public static IServiceCollection AddMemoryCacheInfrastructure(this IServiceCollection services)
    {
        services.AddDistributedMemoryCache();
        services.AddScoped(typeof(CachedRemoteConfiguration));
        services.AddScoped<IRedisResiliencePolicyProvider, RedisResiliencePolicyProvider>();

        return services;
    }
}