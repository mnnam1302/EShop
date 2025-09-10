using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.JsonApi.Middlewares;

namespace EShop.ApiGateway.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddRedisHealthCheck(configuration)
            .AddRedisInfrastructure(configuration);

        services.AddUserTokensProvider();

        return services;
    }

    public static IServiceCollection AddBoostrapping(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ExceptionHandlingMiddleware>();
        services
            .AddCorsApiGateway()
            .AddYarpReverseProxy(configuration)
            .AddAuthenticationApiGateway(configuration);

        return services;
    }

    private static IServiceCollection AddCorsApiGateway(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
            });
        });
        return services;
    }

    private static IServiceCollection AddYarpReverseProxy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddServiceDiscovery();
        services
            .AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"))
            .AddServiceDiscoveryDestinationResolver();

        return services;
    }
}
