using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.HealthChecks;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;

namespace EShop.Shared.JsonApi.DependencyInjections;

public static class UserPermissionsExtensions
{
    public static void RegisterPermissions(this IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, ILogger logger)
    {
        applicationLifetime.ApplicationStarted.Register(() =>
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var healthCheckService = scope.ServiceProvider.GetRequiredService<HealthCheckService>();
                healthCheckService.WaitForHealthyEventBus();

                var instances = scope.ServiceProvider.GetServices<IPermissionRegistrationService>();
                foreach (var instance in instances)
                {
                    logger.LogDebug("Running permission registration for {Service}", instance.GetType().Name);
                    AsyncContext.Run(() => instance.RegisterPermissions());
                }
            }
        });
    }

    public static IServiceCollection AddUserPermissionsProvider(this IServiceCollection services)
    {
        services.AddScoped<IPermissionValidator, CurrentUserPermissionsValidator>();
        AddPermissionCachingService(services);

        return services;
    }

    private static void AddPermissionCachingService(IServiceCollection services)
    {
        services.ConfigureHttpClientDefaults(options =>
        {
            options.AddServiceDiscovery();
        });

        services.AddServiceDiscovery();

        services
            .AddHttpClient<UserPermissionHttpClient>(client =>
            {
                client.BaseAddress = new Uri("http://UsersServiceUrl");
            })
            .AddPolicyHandler(ResilientClientPolicies.GetRetryOnErrorAndNotFoundPolicy())
            .AddPolicyHandler(ResilientClientPolicies.GetCircuitBreakerPolicy());

        services.AddScoped<IUserPermissionsProvider, UserPermissionProvider>();
        services.AddScoped<IPermissionCachingService, PermissionRedisCachingService>();
        services.AddScoped<IRedisCachingAsyncProvider<string[]>, RedisCachingAsyncProvider<string[]>>();
    }
}