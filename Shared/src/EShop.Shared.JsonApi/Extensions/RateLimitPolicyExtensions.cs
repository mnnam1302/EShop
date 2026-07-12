using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Cache.Services;
using EShop.Shared.DomainTools.Extensions;
using Microsoft.Extensions.Configuration;
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
            .AddHttpClient<RateLimitPolicyHttpClient>((serviceProvider, httpClient) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var tenancyServiceUrl = configuration["Services:Tenancy"].Require();
                httpClient.BaseAddress = new Uri(tenancyServiceUrl);
                httpClient.Timeout = TimeSpan.FromSeconds(2);
            })
            .AddPolicyHandler(ResilientClientPolicies.GetRetryOnErrorAndNotFoundPolicy())
            .AddPolicyHandler(ResilientClientPolicies.GetCircuitBreakerPolicy());

        return services;
    }
}
