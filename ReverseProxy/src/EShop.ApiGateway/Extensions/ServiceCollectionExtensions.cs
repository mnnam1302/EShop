using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Authentication.Managers;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.JsonApi.RateLimiting;
using EShop.Shared.RateLimiting.DependencyInjections;
using Microsoft.AspNetCore.HttpOverrides;

namespace EShop.ApiGateway.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddRedisHealthCheck(configuration)
            .AddRedisCacheInfrastructure(configuration)
            .AddTenantAuthenticationProvider()
            .AddCurrentUserAccessor()
            .AddRateLimitPolicyResolver()
            .AddDistributedRateLimiter();

        return services;
    }

    // The gateway previously never registered IUserDetailsProvider at all (AddTenantAuthenticationProvider
    // only wires up JWT validation, not the per-request user/tenant reader) — every rate-limit check that
    // asked "is this user authenticated" silently got null and fell through to the anonymous path, even
    // for genuinely authenticated requests. HttpRequestUserDataProvider is registered directly here
    // (not via the full AddMultiTenantScoping(), which also wires up DB-level tenant isolation the
    // gateway has no database to apply).
    private static IServiceCollection AddCurrentUserAccessor(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserDetailsProvider, HttpRequestUserDataProvider>();

        return services;
    }

    public static IServiceCollection AddBoostrapping(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddGlobalExceptionMiddleware()
            .ConfigureCors(configuration)
            .ConfigureForwardedHeaders()
            .ConfigureRateLimiters()
            .AddServiceDiscovery()
            .AddEndpointsApiExplorer()
            .AddYarpReverseProxy(configuration);

        return services;
    }

    // O2: real client IP for the anonymous-IP login rule requires the gateway to trust the
    // X-Forwarded-For header from whatever sits in front of it (load balancer, CDN, Aspire's
    // container network bridge in local dev). KnownNetworks/KnownProxies are cleared because the
    // exact trusted hop isn't fixed yet for every deployment target — tighten this to the real
    // ingress's IP/CIDR once production topology is confirmed; until then this trusts the immediate
    // upstream's X-Forwarded-For value as-is.
    private static IServiceCollection ConfigureForwardedHeaders(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

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
