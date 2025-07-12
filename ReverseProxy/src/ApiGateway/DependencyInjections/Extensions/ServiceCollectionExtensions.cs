using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.JsonApi.DependencyInjections;
using EShop.Shared.JsonApi.Middlewares;

namespace ApiGateway.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddUserScoping();

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

    // dont't remove, can reference later
    //public static OptionsBuilder<IdentityHttpClientOptions> AddIdentityHttpClientOptions(this IServiceCollection services, IConfiguration section)
    //{
    //    return services
    //        .AddOptions<IdentityHttpClientOptions>()
    //        .Bind(section)
    //        .ValidateDataAnnotations()
    //        .ValidateOnStart();
    //}

    //public static void AddUserApiHttpClient(this IServiceCollection services)
    //{
    //    services.ConfigureHttpClientDefaults(options =>
    //    {
    //        options.AddServiceDiscovery();
    //    });

    //    services
    //        .AddHttpClient("UserService", (provider, client) =>
    //        {
    //            var options = provider.GetRequiredService<IOptionsMonitor<IdentityHttpClientOptions>>().CurrentValue as dynamic;
    //            client.BaseAddress = new Uri(options.BaseAddress);
    //            client.Timeout = TimeSpan.FromSeconds(options.ScoringApiTimeout ?? DefaultExternalServiceTimeoutInSeconds);
    //        })
    //        .AddServiceDiscovery()
    //        .AddPolicyHandler(ResilientClientPolicies.GetRetryOnErrorAndNotFoundPolicy())
    //        .AddPolicyHandler(ResilientClientPolicies.GetCircuitBreakerPolicy());
    //}
}