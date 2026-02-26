using EShop.Shared.JsonApi.Middlewares;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.JsonApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGlobalExceptionMiddleware(this IServiceCollection services)
    {
        services.AddSingleton<ExceptionHandlingMiddleware>();
        return services;
    }

    public static IServiceCollection ConfigureCors(this IServiceCollection services, IConfiguration configuration)
    {
        var corsOptions = new CorsOptions();
        configuration.GetSection(nameof(CorsOptions)).Bind(corsOptions);

        services.AddCors(options =>
        {
            options.AddPolicy(CorsConstants.DevelopmentCorsPolicy, policy =>
            {
                policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
            });

            options.AddPolicy(CorsConstants.ProductionCorsPolicy, policy =>
            {
                policy
                    .WithOrigins(corsOptions.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}

public static class CorsConstants
{
    public const string DevelopmentCorsPolicy = "dev-policy";
    public const string ProductionCorsPolicy = "prod-policy";
}

public sealed class CorsOptions
{
    public string[] AllowedOrigins { get; set; } = [];
}