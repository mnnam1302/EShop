using EShop.Authorization.Application.Services;
using EShop.Authorization.Domain.Services;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.CQRS;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Authorization.Application.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthorizationApplication(this IServiceCollection services)
    {
        services.AddMediator(AssemblyReference.Assembly);

        services.AddApplicationServices();
        services.AddUserPermissionsProvider();
        services.AddUserOrganizationContextProvider();

        return services;
    }

    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IRootOrganizationService, RootOrganizationService>();
    }

    public static void AddUserPermissionsProvider(this IServiceCollection services)
    {
        services.AddTransient<IPermissionValidator, CurrentUserPermissionsValidator>();
        services.AddTransient<IUserPermissionsProvider, OwnerUserPermissionProvider>();
        services.AddTransient<IRedisCachingProvider<string[]>, RedisCachingProvider<string[]>>();
        services.AddTransient<IPermissionCachingService, PermissionRedisCachingService>();
        services.AddTransient<IPermissionCalculator, PermissionCalculator>();
    }

    public static void AddUserOrganizationContextProvider(this IServiceCollection services)
    {
        services.AddScoped<IUserOrganizationContextProvider, OwnerUserOrganizationContextProvider>();
        services.AddScoped<IUserOrganizationContextCalculator, UserOrganizationContextCalculator>();
        services.AddScoped<IUserOrganizationContextCachingService, UserOrganizationContextCachingService>();
        services.AddScoped<IRedisCachingProvider<UserOrganizationContext>, RedisCachingProvider<UserOrganizationContext>>();
        services.AddScoped<IOrganizationContextCachingService, OrganizationContextCachingService>();
        services.AddScoped<IRedisCachingProvider<OrganizationContext>, RedisCachingProvider<OrganizationContext>>();
    }
}
