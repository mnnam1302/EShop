using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using EShop.Testing.JsonApiApplication.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Testing.JsonApiApplication.DependencyInjections;

public static class UserPermissionExtensions
{
    public static IServiceCollection AddTestUserPermissions(this IServiceCollection services)
    {
        services.AddScoped<IPermissionValidator, CurrentUserPermissionsValidator>();
        services.AddSingleton<IUserPermissionsProvider, TestUserPermissionProvider>();

        return services;
    }
}