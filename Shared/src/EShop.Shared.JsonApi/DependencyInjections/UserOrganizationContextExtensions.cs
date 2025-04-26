using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;
using Microsoft.Extensions.DependencyInjection;
using static EShop.Shared.Contracts.Services.Identity.Organizations.Response;
using static EShop.Shared.Contracts.Services.Identity.Users.Response;

namespace EShop.Shared.JsonApi.DependencyInjections;

public static class UserOrganizationContextExtensions
{
    public static IServiceCollection AddUserOrganizationContextProvider(this IServiceCollection services)
    {
        services.AddScoped<IUserOrganizationContextProvider, UserOrganizationContextProvider>();

        services.AddUserOrganizationContextCachingService();
        services.AddUserOrganizationContextHttpClient();

        return services;
    }

    private static void AddUserOrganizationContextCachingService(this IServiceCollection services)
    {
        services.AddScoped<IUserOrganizationContextCachingService, UserOrganizationContextCachingService>();
        services.AddScoped<IRedisCachingAsyncProvider<UserOrganizationContext>, RedisCachingAsyncProvider<UserOrganizationContext>>();

        services.AddScoped<IOrganizationContextCachingService, OrganizationContextCachingService>();
        services.AddScoped<IRedisCachingAsyncProvider<OrganizationContext>, RedisCachingAsyncProvider<OrganizationContext>>();
    }

    private static void AddUserOrganizationContextHttpClient(this IServiceCollection services)
    {
        services.ConfigureHttpClientDefaults(options =>
        {
            options.AddServiceDiscovery();
        });
        services.AddServiceDiscovery();

        services
            .AddHttpClient<UserOrganizationContextHttpClient>(client =>
            {
                client.BaseAddress = new Uri("http://UsersServiceUrl");
            })
            .AddPolicyHandler(ResilientClientPolicies.GetRetryOnErrorAndNotFoundPolicy())
            .AddPolicyHandler(ResilientClientPolicies.GetCircuitBreakerPolicy());
    }
}