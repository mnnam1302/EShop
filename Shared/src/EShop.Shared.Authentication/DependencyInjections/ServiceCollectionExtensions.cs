using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Authentication.Managers.JwtTokens;
using EShop.Shared.Authentication.Managers.RsaKey;
using EShop.Shared.Authentication.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.Authentication.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTenantAuthentication(this IServiceCollection services)
    {
        services.AddOptions<JwtOptions>()
            .BindConfiguration(nameof(JwtOptions))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<ITenantKeyProvider, TenantKeyProvider>();
        services.AddScoped<IJwtTokenManager, JwtTokenManager>();
        services.AddScoped<ISystemInternalJwtTokenFactory, SystemInternalJwtTokenFactory>();
        services.AddAuthenticationHandler();

        return services;
    }

    public static IServiceCollection AddAuthenticationHandler(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddScheme<JwtBearerOptions, MultiTenantJwtBearerHandler>(JwtBearerDefaults.AuthenticationScheme, options => { });

        services.AddAuthorization();
        services.AddAuthorizationBuilder()
            .AddPolicy("authPolicy", policy => policy.RequireAuthenticatedUser());

        return services;
    }
}
