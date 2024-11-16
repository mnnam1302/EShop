using EShop.Identity.Application.Services;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.Contracts.Services.Identity.Auth;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;

namespace EShop.Identity.API.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTokenCachingServiceForOwnerServiceAPI(this IServiceCollection services)
    {
        services.AddTransient<
            IRedisCachingProvider<Response.AuthenticatedResponse>,
            RedisCachingProvider<Response.AuthenticatedResponse>>();

        services.AddTransient<ITokenCachingService, TokenRedisCachingService>();

        return services;
    }

    public static IServiceCollection AddUserPermissionForOwnerServiceAPI(
        this IServiceCollection services)
    {
        services.AddTransient<IPermissionValidator, CurrentUserPermissionsValidator>();
        AddPermissionCachingServiceForOwnService(services);
        return services;
    }

    private static void AddPermissionCachingServiceForOwnService(IServiceCollection services)
    {
        //var redisOptions = new RedisOptions();
        //configuration.GetSection(nameof(RedisOptions)).Bind(redisOptions);

        //services.AddSingleton(redisOptions);

        //if (!redisOptions.Enabled)
        //    return;

        //services.AddStackExchangeRedisCache(options =>
        //{
        //    var connectionString = redisOptions.ConnectionString;
        //    options.Configuration = connectionString;
        //});

        services.AddTransient(typeof(CachedRemoteConfiguration));
        services.AddTransient<IRedisResiliencePolicyProvider, RedisResiliencePolicyProvider>();

        services.AddTransient<IRedisCachingProvider<string[]>, RedisCachingProvider<string[]>>();
        services.AddTransient<IPermissionCachingOwnerService, PermissionRedisCachingService>();
        services.AddTransient<IUserPermissionsProvider, OwnerCacheUserPermissionService>();
    }
}