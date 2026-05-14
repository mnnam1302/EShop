using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.DomainTools.Extensions;
using EShop.Shared.HealthChecks;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;

namespace EShop.Shared.JsonApi.Extensions;

public static class UserPermissionsExtensions
{
    public static void RegisterPermissions(this IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, ILogger logger)
    {
        applicationLifetime.ApplicationStarted.Register(() =>
        {
            using var scope = app.ApplicationServices.CreateScope();
            var healthCheckService = scope.ServiceProvider.GetRequiredService<HealthCheckService>();
            healthCheckService.WaitForHealthyEventBus();

            var instances = scope.ServiceProvider.GetServices<IPermissionRegistrationService>();
            foreach (var instance in instances)
            {
                logger.LogDebug("Running permission registration for {Service}", instance.GetType().Name);
                AsyncContext.Run(() => instance.RegisterPermissions());
            }
        });
    }

    public static IServiceCollection AddUserPermissionsProvider(this IServiceCollection services)
    {
        services
            .AddScoped<IPermissionValidator, CurrentUserPermissionsValidator>()
            .AddPermissionCachingService()
            .AddUserPermissionHttpClient();

        return services;
    }

    private static IServiceCollection AddPermissionCachingService(this IServiceCollection services)
    {
        services.AddScoped<IUserPermissionsProvider, UserPermissionProvider>();
        services.AddScoped<IPermissionCachingService, PermissionRedisCachingService>();
        services.AddScoped<IRedisCachingProvider<string[]>, RedisCachingProvider<string[]>>();

        return services;
    }

    private static IServiceCollection AddUserPermissionHttpClient(this IServiceCollection services)
    {
        services.AddServiceDiscovery();
        services.ConfigureHttpClientDefaults(options =>
        {
            // Turn on service discovery
            options.AddServiceDiscovery();
        });

        services
            .AddHttpClient<UserPermisssionHttpClient>((serviceProvider, httpClient) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var authorizationServiceUrl = configuration["Services:Authorization"].Require();
                httpClient.BaseAddress = new Uri(authorizationServiceUrl);
            })
            .AddPolicyHandler(ResilientClientPolicies.GetRetryOnErrorAndNotFoundPolicy())
            .AddPolicyHandler(ResilientClientPolicies.GetCircuitBreakerPolicy());

        return services;
    }
}
