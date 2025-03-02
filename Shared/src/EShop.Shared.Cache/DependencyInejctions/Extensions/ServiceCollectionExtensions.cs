using EShop.Shared.Cache.DependencyInejctions.Options;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.Contracts.Services.Identity.Auth;
using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.Cache.DependencyInejctions.Extensions;

public static class ServiceCollectionExtensions
{
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

        services.AddTransient(typeof(CachedRemoteConfiguration));
        services.AddTransient<IRedisResiliencePolicyProvider, RedisResiliencePolicyProvider>();

        return services;
    }
    public static IServiceCollection AddMemoryInfrastructure(this IServiceCollection services)
    {
        services.AddDistributedMemoryCache();
        services.AddTransient(typeof(CachedRemoteConfiguration));
        services.AddTransient<IRedisResiliencePolicyProvider, RedisResiliencePolicyProvider>();

        return services;
    }

    public static IServiceCollection AddRedisCachingService(this IServiceCollection services)
    {
        services.AddTransient(typeof(IRedisCachingAsyncService<>), typeof(RedisCachingAsyncService<>));
        return services;
    }

    public static IServiceCollection AddUserTokenCachingService(this IServiceCollection services)
    {
        services.AddTransient<ITokenCachingService, TokenRedisCachingService>();
        return services;
    }
}