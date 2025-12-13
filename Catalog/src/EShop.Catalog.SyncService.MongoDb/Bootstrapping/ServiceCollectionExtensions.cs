using EShop.Catalog.SyncService.MongoDb.Bootstrapping;
using EShop.Catalog.SyncService.MongoDb.Infrastructure;
using EShop.Catalog.SyncService.MongoDb.Infrastructure.Repository;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Extensions;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EShop.Catalog.SyncService.MongoDb.Bootstrapping;

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
        services.AddOptions<MongoDbSettings>().BindConfiguration(nameof(MongoDbSettings));
        services.AddSingleton<IMongoDbSettings>(sp => sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);

        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IMongoDbSettings>();
            return new MongoClient(settings.ConnectionString);
        });

        services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

        return services;
    }
}
