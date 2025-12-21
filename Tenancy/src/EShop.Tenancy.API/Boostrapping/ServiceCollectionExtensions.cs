using Carter;
using EShop.Shared.DomainTools.Extensions;
using EShop.Shared.JsonApi.Extensions;
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

        services
            .AddTenantAuthenticationProvider()
            .AddUserPermissionsProvider()
            .AddUserOrganizationContextProvider();

        return services;
    }

    public static IServiceCollection AddTenancyAPI(this IServiceCollection services)
    {
        services.AddEshopCors();
        services.AddResiliencePolicy();
        services.AddGlobalExceptionMiddleware();

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

        return services;
    }
}