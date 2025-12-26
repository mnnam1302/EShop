using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.DomainTools.Extensions;
using EShop.Shared.HealthChecks;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;

namespace EShop.Shared.JsonApi.Extensions;

public static class TenantFeaturesExtensions
{
    public static void RegisterFeatures(this IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, ILogger logger)
    {
        applicationLifetime.ApplicationStarted.Register(() =>
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var healthCheckService = scope.ServiceProvider.GetRequiredService<HealthCheckService>();
                healthCheckService.WaitForHealthyEventBus();

                var instances = scope.ServiceProvider.GetServices<IFeatureRegistrationService>();

                foreach (var instance in instances)
                {
                    logger.LogInformation("Running feature registration for {Service}", instance.GetType().Name);
                    AsyncContext.Run(instance.RegisterFeatures);
                }
            }
        });
    }

    public static IServiceCollection AddTenantFeaturesProvider(this IServiceCollection services)
    {
        services
            .AddScoped<IFeatureValidator, CurrentUserFeaturesValidator>()
            .AddTenantFeatureCachingService()
            .AddTenantFeaturesHttpClient();

        return services;
    }

    private static IServiceCollection AddTenantFeatureCachingService(this IServiceCollection services)
    {
        services.AddScoped<ITenantFeaturesProvider, TenantFeaturesProvider>();
        services.AddScoped<ITenantFeaturesCachingService, TenantFeaturesRedisCachingService>();
        services.AddScoped<IRedisCachingProvider<string[]>, RedisCachingProvider<string[]>>();

        return services;
    }

    private static IServiceCollection AddTenantFeaturesHttpClient(this IServiceCollection services)
    {
        services.AddServiceDiscovery();
        services.ConfigureHttpClientDefaults(configure =>
        {
            // Turn on service discovery
            configure.AddServiceDiscovery();
        });

        services
            .AddHttpClient<TenancyHttpClient>((serviceProvider, httpClient) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var tenancyServiceUrl = configuration["Services:Tenancy"].Require();
                httpClient.BaseAddress = new Uri(tenancyServiceUrl);
            })
            .AddPolicyHandler(ResilientClientPolicies.GetRetryOnErrorAndNotFoundPolicy())
            .AddPolicyHandler(ResilientClientPolicies.GetCircuitBreakerPolicy());

        return services;
    }
}