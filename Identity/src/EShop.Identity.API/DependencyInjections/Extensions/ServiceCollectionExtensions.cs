using EShop.Identity.Application.Services;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;

namespace EShop.Identity.API.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserPermissionForOwnerServiceAPI(
        this IServiceCollection services)
    {
        services.AddTransient<IPermissionValidator, CurrentUserPermissionsValidator>();
        AddPermissionCachingServiceForOwnService(services);
        return services;
    }

    private static void AddPermissionCachingServiceForOwnService(IServiceCollection services)
    {
        services.AddTransient<IRedisCachingProvider<string[]>, RedisCachingProvider<string[]>>();
        services.AddTransient<IPermissionCachingOwnerService, PermissionRedisCachingService>();
        services.AddTransient<IPermissionCalculator, PermissionCalculator>();
        services.AddTransient<IUserPermissionsProvider, OwnerCacheUserPermissionService>();
    }
}