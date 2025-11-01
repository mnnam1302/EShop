using EShop.Shared.Authentication.DependencyInjections;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.JsonApi.Extensions;

public static class TenantAuthenticationExtensions
{
    public static IServiceCollection AddTenantAuthenticationProvider(this IServiceCollection services)
    {
        services.AddTenantKeyCachingServices();
        services.AddUserTokensCachingServices();

        services.AddTenantAuthentication();

        return services;
    }
}
