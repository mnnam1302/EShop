using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

namespace EShop.Configuration.Application.Boostrapping;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBoostrapping(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddCors()
            .AddSwagger()
            .AddApiVersioning()
            .AddServiceBootstrapping();

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

    private static IServiceCollection AddApiVersioning(this IServiceCollection services)
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

    private static IServiceCollection AddServiceBootstrapping(this IServiceCollection services)
    {
        services.AddTransient<DbInitializer>();
        return services;
    }
}
