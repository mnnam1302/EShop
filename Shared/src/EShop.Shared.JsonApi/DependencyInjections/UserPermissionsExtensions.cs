using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.JsonApi.DependencyInjections;

public static class UserPermissionsExtensions
{
    //public static IServiceCollection RegisterPermissions(this IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, ILogger logger)
    //{
    //    return services;
    //}

    public static IServiceCollection AddUserPermissionsProvider(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPermissionValidator, CurrentUserPermissionsValidator>();
        AddPermissionHttpClient(services, configuration);
        AddPermissionCachingService(services, configuration);

        return services;
    }
    private static void AddPermissionHttpClient(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHttpClient<UserPermissionHttpClient>(client =>
            {
                client.BaseAddress = configuration.GetSection("ExternalServices").GetValue<Uri>("UsersApiUrl");
            })
            .AddPolicyHandler(ResilientClientPolicies.GetRetryOnErrorAndNotFoundPolicy())
            .AddPolicyHandler(ResilientClientPolicies.GetCircuitBreakerPolicy());
    }

    private static void AddPermissionCachingService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRedisCachingAsyncProvider<string[]>, RedisCachingAsyncProvider<string[]>>();
        services.AddScoped<IPermissionCachingService, PermissionRedisCachingService>();
        services.AddScoped<IUserPermissionsProvider, UserPermissionProvider>();
    }
}