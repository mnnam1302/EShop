using EShop.Authorization.Application.Services;
using EShop.Authorization.Domain.Services;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Authorization.Application.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthorizationApplication(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IRootOrganizationService, RootOrganizationService>();
        services.AddUserPermissionForOwnerService();

        return services;
    }

    public static void AddUserPermissionForOwnerService(this IServiceCollection services)
    {
        services.AddTransient<IPermissionValidator, CurrentUserPermissionsValidator>();

        services.AddTransient<IUserPermissionsProvider, OwnerCacheUserPermissionService>();
        services.AddTransient<IRedisCachingProvider<string[]>, RedisCachingProvider<string[]>>();
        services.AddTransient<IPermissionCachingService, PermissionRedisCachingService>();
        services.AddTransient<IPermissionCalculator, PermissionCalculator>();
    }
}
