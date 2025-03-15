using EShop.Shared.HealthChecks;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;

namespace EShop.Shared.Scoping.DependencyInjections.Extensions;

public static class FeatureSExtensions
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

    public static IServiceCollection AddUserFeaturesProvider(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddFeatureCachingService(this IServiceCollection services)
    {
        return services;
    }
}