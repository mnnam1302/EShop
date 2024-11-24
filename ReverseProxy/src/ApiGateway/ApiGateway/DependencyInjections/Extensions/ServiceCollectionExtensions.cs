using ApiGateway.DependencyInjections.Options;
using Microsoft.Extensions.Options;

namespace ApiGateway.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
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
        // Research other way to config: https://dev.to/leandroveiga/building-a-centralized-api-proxy-with-yarp-in-net-8-using-minimal-apis-1hna
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"));
        return services;
    }

    public static OptionsBuilder<IdentityHttpClientOptions> AddIdentityGrpcClientOptions(this IServiceCollection services, IConfiguration section)
    {
        return services
            .AddOptions<IdentityHttpClientOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    //private static void AddHttpClient<TClient, TOptions>(this IServiceCollection services)
    //    where TClient : ClientBase<TClient>
    //    where TOptions : class
    //{
    //    services
    //        .AddHttpClient<TClient>((provider, client) =>
    //        {
    //            var options = provider.GetRequiredService<IOptionsMonitor<TOptions>>().CurrentValue as dynamic;
    //            client.Address = new Uri(options.BaseAddress);
    //        })
    //        .AddCorrelationIdForwarding()
    //        .ConfigureChannel(options =>
    //        {
    //            options.Credentials = ChannelCredentials.Insecure;
    //            options.ServiceConfig = new() { LoadBalancingConfigs = { new RoundRobinConfig() } };
    //        })
    //        .ConfigurePrimaryHttpMessageHandler(() =>
    //            new SocketsHttpHandler
    //            {
    //                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
    //                KeepAlivePingDelay = TimeSpan.FromSeconds(60),
    //                KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
    //                EnableMultipleHttp2Connections = true
    //            })
    //        .EnableCallContextPropagation(options => options.SuppressContextNotFoundErrors = true)
    //        .AddPolicyHandler(ResilientClientPolicies.GetRetryOnErrorAndNotFoundPolicy())
    //        .AddPolicyHandler(ResilientClientPolicies.GetCircuitBreakerPolicy());
    //}
}