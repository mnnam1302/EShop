using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Cache.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.JsonApi.Extensions;

public static class RateLimitPolicyExtensions
{
    public static IServiceCollection AddRateLimitPolicyResolver(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddRateLimitPolicyCachingService();
        services.AddScoped<IRateLimitPolicyResolver, RateLimitPolicyResolver>();
        services.AddSingleton<IRateLimitRuleResolver, RateLimitRuleResolver>();
        services.AddRateLimitPolicyHttpClient();

        return services;
    }

    private static IServiceCollection AddRateLimitPolicyHttpClient(this IServiceCollection services)
    {
        services.AddServiceDiscovery();
        services.ConfigureHttpClientDefaults(configure =>
        {
            configure.AddServiceDiscovery();
        });

        services
            .AddHttpClient<RateLimitPolicyHttpClient>(httpClient =>
            {
                // "tenancy" is a service name, resolved dynamically via AddServiceDiscovery() from
                // the Services:tenancy:http config section (the same one YARP's own cluster
                // destinations use) — not a literal URL.
                httpClient.BaseAddress = new Uri("http://tenancy");
                httpClient.Timeout = TimeSpan.FromSeconds(2);
            })
            .AddPolicyHandler(ResilientClientPolicies.GetRetryOnErrorAndNotFoundPolicy())
            .AddPolicyHandler(ResilientClientPolicies.GetCircuitBreakerPolicy());

        return services;
    }
}
