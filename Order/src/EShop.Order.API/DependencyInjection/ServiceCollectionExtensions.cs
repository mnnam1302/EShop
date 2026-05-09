using EShop.Shared.JsonApi.Extensions;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

namespace EShop.Order.API.DependencyInjection;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderAPI(this IServiceCollection services)
    {
        services
            .AddCors()
            .AddGlobalExceptionMiddleware()
            .AddSwagger()
            .AddOrderApiVersioning();

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

    private static IServiceCollection AddOrderApiVersioning(this IServiceCollection services)
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