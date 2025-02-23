using ApiGateway.Middlewares;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.JsonApi.DependencyInjections;

namespace ApiGateway.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    internal const string ApplicationName = "ApiGateway";

    public static IServiceCollection AddBoostrapping(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ExceptionHandlingMiddleware>();
        services
            .AddCorsApiGateway()
            //.AddConsul()
            .AddYarpReverseProxy(configuration)
            .AddAuthenticationApiGateway(configuration);

        return services;
    }

    private static IServiceCollection AddCorsApiGateway(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });
        return services;
    }

    //private static IServiceCollection AddConsul(this IServiceCollection services)
    //{
    //    // Read more document: https://docs.steeltoe.io/api/v3/discovery/hashicorp-consul.html
    //    services.AddServiceDiscovery(o => o.UseConsul());
    //    return services;
    //}

    private static IServiceCollection AddYarpReverseProxy(this IServiceCollection services, IConfiguration configuration)
    {
        /*
         *  Old code apply service discovery destination resolver for Yarp Reverse Proxy
         *  Microsoft.Extensions.ServiceDiscovery
         *  Microsoft.Extensions.ServiceDiscovery.Yarp
         */
        services.AddServiceDiscovery();
        services
            .AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"))
            .AddServiceDiscoveryDestinationResolver();

        return services;
    }

    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddUserScoping()
            .AddRedisInfrastructure(configuration)
            .AddUserTokenCachingService();

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