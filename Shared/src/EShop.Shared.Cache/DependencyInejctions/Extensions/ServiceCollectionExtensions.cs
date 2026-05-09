using EShop.Shared.Cache.DependencyInejctions.Options;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Diagnostics;
using EShop.Shared.DomainTools.Extensions;
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

        var connectionString = configuration.IsRunningInAspire()
            ? configuration.GetConnectionString("redis").Require()
            : redisOptions.ConnectionString;

        services
            .AddHealthChecks()
            .AddRedis(connectionString, "redis", HealthStatus.Degraded, ["cache", "redis"]);

        return services;
    }

    public static IServiceCollection AddRedisCacheInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisOptions>(configuration.GetSection(nameof(RedisOptions)));

        var redisOptions = configuration
            .GetSection(nameof(RedisOptions))
            .Get<RedisOptions>()
            .Require();

        if (!redisOptions.Enabled)
            return services;

        var connectionString = configuration.IsRunningInAspire()
            ? configuration.GetConnectionString("redis").Require()
            : redisOptions.ConnectionString;

        services.AddStackExchangeRedisCache(options => options.Configuration = connectionString);

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
