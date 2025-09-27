using EShop.Authorization.Application.Services;
using EShop.Authorization.Infrastructure.DependencyInjection;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.CQRS;
using EShop.Shared.DomainTools.DependencyInjections;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.JsonApi.Middlewares;
using MassTransit;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

namespace EShop.Authorization.Application.Boostrapping;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddResiliencePolicy();

        services
            .AddRedisHealthCheck(configuration)
            .AddRedisInfrastructure(configuration);

        services
            .AddUserTokensProvider()
            .AddTenantFeaturesProvider();

        services.AddMediator(AssemblyReference.Assembly);

        return services;
    }

    public static IServiceCollection AddBoostrapping(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services
            .AddApiServices()
            .AddApplicationServices()
            .AddInfrastructure(configuration, environment);

        return services;
    }

    private static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddCors();
        services.AddSingleton<ExceptionHandlingMiddleware>();

        services.AddSwaggerGenNewtonsoftSupport()
            .AddFluentValidationRulesToSwagger()
            .AddEndpointsApiExplorer()
            .AddSwaggerAPI();

        services
            .AddApiVersioning(options => options.ReportApiVersions = true)
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        return services;
    }
}
