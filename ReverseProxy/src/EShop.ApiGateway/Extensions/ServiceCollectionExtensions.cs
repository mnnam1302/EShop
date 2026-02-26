using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.JsonApi.RateLimiting;

namespace EShop.ApiGateway.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddRedisHealthCheck(configuration)
            .AddRedisCacheInfrastructure(configuration)
            .AddTenantAuthenticationProvider();

        return services;
    }

    public static IServiceCollection AddBoostrapping(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddGlobalExceptionMiddleware()
            .ConfigureCors(configuration)
            .ConfigureRateLimiters()
            .AddServiceDiscovery()
            .AddEndpointsApiExplorer()
            .AddYarpReverseProxy(configuration);

        return services;
    }

    private static IServiceCollection AddYarpReverseProxy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"))
            .AddServiceDiscoveryDestinationResolver();

        return services;
    }
}
