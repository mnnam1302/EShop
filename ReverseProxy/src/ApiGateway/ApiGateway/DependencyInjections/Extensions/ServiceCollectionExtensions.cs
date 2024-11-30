using ApiGateway.DependencyInjections.Options;
using EShop.Shared.JsonApi;
using Microsoft.Extensions.Options;

namespace ApiGateway.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    //private const int DefaultExternalServiceTimeoutInSeconds = 30;

    public static IServiceCollection AddCorsApiGateway(this IServiceCollection services)
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

    public static IServiceCollection AddYarpReverseProxy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddServiceDiscovery();

        // Research other way to config: https://dev.to/leandroveiga/building-a-centralized-api-proxy-with-yarp-in-net-8-using-minimal-apis-1hna
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"))
            .AddServiceDiscoveryDestinationResolver();
        return services;
    }

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