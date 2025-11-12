using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Authentication.Managers.JwtTokens;
using EShop.Shared.Authentication.Managers.RsaKey;
using EShop.Shared.Authentication.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
        }).AddScheme<JwtBearerOptions, MultiTenantJwtBearerHandler>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception?.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.TryAdd("IS-TOKEN-EXPIRED", "true");
                    }

                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAuthenticatedUser", policy =>
            {
                policy.RequireAuthenticatedUser();
            });
        });

        return services;
    }
}
