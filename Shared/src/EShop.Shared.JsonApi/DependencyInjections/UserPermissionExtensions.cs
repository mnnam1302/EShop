using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Shared.JsonApi.DependencyInjections;

public static class UserPermissionExtensions
{
    public static IServiceCollection AddUserPermissionsProvider(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IPermissionValidator, CurrentUserPermissionsValidator>();
        AddPermissionCachingService(services, configuration);

        return services;
    }

    private static void AddPermissionCachingService(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHttpClient<UserPermissionHttpClient>(client =>
            {
                client.BaseAddress = configuration.GetSection("ExternalServices").GetValue<Uri>("UsersApiUrl");
            })
            .AddPolicyHandler(ResilientClientPolicies.GetRetryOnErrorAndNotFoundPolicy())
            .AddPolicyHandler(ResilientClientPolicies.GetCircuitBreakerPolicy());

        services.AddRedisInfrastructure(configuration);
        services.AddTransient<IRedisResiliencePolicyProvider, RedisResiliencePolicyProvider>();
        services.AddTransient<IRedisCachingProvider<string[]>, RedisCachingProvider<string[]>>();
        services.AddTransient<IPermissionCachingService, PermissionRedisCachingService>();
        services.AddTransient<IUserPermissionsProvider, CacheUserPermissionService>();
    }
}