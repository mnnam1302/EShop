using EShop.Authorization.Application.DependencyInjections;
using EShop.Authorization.Infrastructure.DependencyInjections;
using EShop.Shared.DomainTools.Extensions;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.JsonApi.Middlewares;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

namespace EShop.Authorization.API.Boostrapping;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBoostrapping(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddAuthorizationAPI()
            .AddAuthorizationApplication()
            .AddAuthorizationPersistence(configuration)
            .AddAuthorizationInfrastructure(configuration, environment);

        return services;
    }

    public static IServiceCollection AddAuthorizationAPI(this IServiceCollection services)
    {
        services.AddCors();
        services.AddResiliencePolicy();
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

        services.AddTenantAuthenticationProvider();

        return services;
    }
}