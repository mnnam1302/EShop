using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.DomainTools.Extensions;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.JsonApi.Extensions;

public static class UserOrganizationContextExtensions
{
    public static IServiceCollection AddUserOrganizationContextProvider(this IServiceCollection services)
    {
        services
            .AddScoped<IUserOrganizationContextProvider, UserOrganizationContextProvider>()
            .AddUserOrganizationContextCachingService()
            .AddUserOrganizationContextHttpClient();

        return services;
    }

    private static IServiceCollection AddUserOrganizationContextCachingService(this IServiceCollection services)
    {
        services.AddScoped<IUserOrganizationContextCachingService, UserOrganizationContextCachingService>();
        services.AddScoped<IRedisCachingProvider<UserOrganizationContext>, RedisCachingProvider<UserOrganizationContext>>();
        services.AddScoped<IOrganizationContextCachingService, OrganizationContextCachingService>();
        services.AddScoped<IRedisCachingProvider<OrganizationContext>, RedisCachingProvider<OrganizationContext>>();

        return services;
    }

    private static IServiceCollection AddUserOrganizationContextHttpClient(this IServiceCollection services)
    {
        services
            .ConfigureHttpClientDefaults(options =>
            {
                options.AddServiceDiscovery();
            })
            .AddServiceDiscovery();

        services
            .AddHttpClient<UserOrganizationContextHttpClient>((serviceProvider, client) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var authorizationServiceUrl = configuration["Services:Authorization"].Require();
                client.BaseAddress = new Uri(authorizationServiceUrl);
            })
            .AddPolicyHandler(ResilientClientPolicies.GetRetryOnErrorAndNotFoundPolicy())
            .AddPolicyHandler(ResilientClientPolicies.GetCircuitBreakerPolicy());

        return services;
    }
}