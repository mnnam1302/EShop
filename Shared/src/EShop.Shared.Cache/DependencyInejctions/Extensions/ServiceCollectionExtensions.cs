using EShop.Shared.Cache.DependencyInejctions.Options;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.Cache.DependencyInejctions.Extensions;

public static class ServiceCollectionExtensions
{
    private static readonly string[] tags = new[] { "cache", "redis" };

    public static IServiceCollection AddRedisHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        var redisOptions = new RedisOptions();
        configuration.GetSection(nameof(RedisOptions)).Bind(redisOptions);

        if (!redisOptions.Enabled)
            return services;

        services.AddHealthChecks()
            .AddRedis(
                redisConnectionString: redisOptions.ConnectionString,
                name: "redis",
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
                tags: tags);

        return services;
    }

    public static IServiceCollection AddRedisInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var redisOptions = new RedisOptions();
        configuration.GetSection(nameof(RedisOptions)).Bind(redisOptions);
        services.AddSingleton(redisOptions);

        if (!redisOptions.Enabled)
            return services;

        services.AddStackExchangeRedisCache(options =>
        {
            var connectionString = redisOptions.ConnectionString;
            options.Configuration = connectionString;
        });

        services.AddScoped(typeof(CachedRemoteConfiguration));
        services.AddScoped<IRedisResiliencePolicyProvider, RedisResiliencePolicyProvider>();

        return services;
    }

    public static IServiceCollection AddMemoryInfrastructure(this IServiceCollection services)
    {
        services.AddDistributedMemoryCache();
        services.AddScoped(typeof(CachedRemoteConfiguration));
        services.AddScoped<IRedisResiliencePolicyProvider, RedisResiliencePolicyProvider>();

        return services;
    }
}