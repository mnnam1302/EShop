using Carter;
using EShop.Shared.DomainTools.DependencyInjections;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.JsonApi.Middlewares;
using EShop.Tenancy.Application.DependencyInjections;
using EShop.Tenancy.Infrastructure.DependencyInjections;
using EShop.Tenancy.Persistence.DependencyInjections;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

namespace EShop.Tenancy.API.Boostrapping;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBoostrapping(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddTenancyAPI()
            .AddTenancyApplication()
            .AddTenancyPersistence(configuration)
            .AddTenancyInfrastructure(configuration, environment, Program.ApplicationName);

        return services;
    }

    public static IServiceCollection AddTenancyAPI(this IServiceCollection services)
    {
        services.AddCors();
        services.AddResiliencePolicy();

        services.AddSingleton<ExceptionHandlingMiddleware>();
        services.AddTransient<DbInitializer>();

        services.AddCarter();

        services
            .AddSwaggerGenNewtonsoftSupport()
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

        services.AddTenantAuthenticationProvider();

        return services;
    }
}