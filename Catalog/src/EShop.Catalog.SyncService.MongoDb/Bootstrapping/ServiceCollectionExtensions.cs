using EShop.Catalog.SyncService.MongoDb.Bootstrapping;
using EShop.Catalog.SyncService.MongoDb.Infrastructure;
using EShop.Catalog.SyncService.MongoDb.Infrastructure.Repository;
using EShop.Catalog.SyncService.MongoDb.Models;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Extensions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Repositories;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            .AddMongoDbPersistence()
            .AddJsonApiDotNet();

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

        services.TryAddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<IMongoDbSettings>();
            var client = new MongoClient(settings.ConnectionString);

            return client.GetDatabase(settings.DatabaseName);
        });

        services.AddScoped(typeof(IMongoRepositoryBase<>), typeof(MongoRepositoryBase<>));

        return services;
    }

    public static IServiceCollection AddJsonApiDotNet(this IServiceCollection services)
    {
        services.AddJsonApi(options =>
        {
            options.Namespace = "api/v1";
            options.UseRelativeLinks = true;
            options.IncludeTotalResourceCount = true;

#if DEBUG
            options.IncludeExceptionStackTraceInErrors = true;
            options.IncludeRequestBodyInErrors = true;
            options.SerializerOptions.WriteIndented = true;
#endif
        }, resources: resourceGraphBuilder =>
        {
            resourceGraphBuilder.Add<CategoryProjection, string?>();
        });

        //If your API project uses MongoDB only(so not in combination with EF Core),
        //then instead of registering all MongoDB resources and repositories individually, you can use:
        services.AddJsonApiMongoDb();

        services.AddScoped(typeof(IResourceReadRepository<,>), typeof(MongoRepository<,>));
        services.AddScoped(typeof(IResourceWriteRepository<,>), typeof(MongoRepository<,>));
        services.AddScoped(typeof(IResourceRepository<,>), typeof(MongoRepository<,>));

        return services;
    }
}