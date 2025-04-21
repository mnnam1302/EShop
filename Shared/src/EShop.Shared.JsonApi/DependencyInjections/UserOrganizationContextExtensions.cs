using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.JsonApi.DependencyInjections;

public static class UserOrganizationContextExtensions
{
    public static IServiceCollection AddUserOrganizationContextProvider(this IServiceCollection services)
    {
        services.AddUserOrganizationContextCachingService();

        return services;
    }

    private static void AddUserOrganizationContextCachingService(this IServiceCollection services)
    {
        //services.AddScoped<IUserOrganizationContextProvider, >
    }
}