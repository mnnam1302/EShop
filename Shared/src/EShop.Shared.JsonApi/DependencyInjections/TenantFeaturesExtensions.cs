using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.HealthChecks;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;

namespace EShop.Shared.JsonApi.DependencyInjections;

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
                    logger.LogInformation("Running feature registration for {service}", instance.GetType().Name);
                    AsyncContext.Run(instance.RegisterFeatures);
                }
            }
        });
    }

    public static IServiceCollection AddTenantFeaturesProvider(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IFeatureValidator, CurrentUserFeaturesValidator>();
        AddTenantFeatureCachingService(services, configuration);

        return services;
    }

    private static void AddTenantFeatureCachingService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddServiceDiscovery();
        services.ConfigureHttpClientDefaults(options =>
        {
            options.AddServiceDiscovery();
        });

        services.AddHttpClient<TenantFeaturesHttpClient>(client =>
        {
            client.BaseAddress = new Uri("http://TenancyServiceUrl");
        })
            .AddPolicyHandler(ResilientClientPolicies.GetRetryOnErrorAndNotFoundPolicy())
            .AddPolicyHandler(ResilientClientPolicies.GetCircuitBreakerPolicy());

        services.AddScoped<ITenantFeaturesProvider, TenantFeaturesProvider>();
        services.AddScoped<ITenantFeaturesCachingService, TenantFeaturesRedisCachingService>();
        services.AddScoped<IRedisCachingAsyncProvider<string[]>, RedisCachingAsyncProvider<string[]>>();
    }
}