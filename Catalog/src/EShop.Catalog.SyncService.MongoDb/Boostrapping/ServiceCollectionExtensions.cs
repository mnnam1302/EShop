using EShop.Catalog.SyncService.MongoDb.Abstractions;
using EShop.Catalog.SyncService.MongoDb.Infrastructure;
using EShop.Catalog.SyncService.MongoDb.Infrastructure.Repository;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.JsonApi.Middlewares;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.Extensions.Options;

namespace EShop.Catalog.SyncService.MongoDb.Boostrapping;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services)
    {
        services
            .GlobalExceptionHandlingMiddleware()
            .AddMediator(AssemblyReference.Assembly);
        return services;
    }

    public static IServiceCollection AddBoostrapping(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        services.AddCors()
            .AddSwagger()
            .AddApiVersioning()
            .AddMassTransitRabbitMQ(configuration, webHostEnvironment)
            .AddMongoDbPersistence();

        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services
            .AddSwaggerGenNewtonsoftSupport()
            .AddFluentValidationRulesToSwagger()
            .AddEndpointsApiExplorer()
            .AddSwaggerAPI();

        return services;
    }

    public static IServiceCollection AddApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options => options.ReportApiVersions = true)
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        return services;
    }

    public static IServiceCollection AddMongoDbPersistence(this IServiceCollection services)
    {
        services.AddOptions<MongoDbSettings>();
        services.AddSingleton<IMongoDbSettings>(sp => sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);

        services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

        return services;
    }
}
