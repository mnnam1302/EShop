using EShop.Shared.JsonApi.Extensions;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

namespace EShop.Inventory.API.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInventoryAPI(this IServiceCollection services)
    {
        services
            .AddCors()
            .AddGlobalExceptionMiddleware()
            .AddSwagger()
            .AddInventoryApiVersioning();

        return services;
    }

    private static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services
            .AddSwaggerGenNewtonsoftSupport()
            .AddFluentValidationRulesToSwagger()
            .AddEndpointsApiExplorer()
            .AddSwaggerAPI();

        return services;
    }

    private static IServiceCollection AddInventoryApiVersioning(this IServiceCollection services)
    {
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