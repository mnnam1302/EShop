using EShop.Catalog.ReadModels.MongoDb.Bootstrapping;
using EShop.Catalog.ReadModels.MongoDb.Consumers;
using EShop.Catalog.ReadModels.MongoDb.Infrastructure;
using EShop.Catalog.ReadModels.MongoDb.Infrastructure.Repository;
using EShop.Catalog.ReadModels.MongoDb.Models;
using EShop.Shared.Contracts.JsonConverters;
using EShop.Shared.Contracts.Services.Catalog;
using EShop.Shared.CQRS;
using EShop.Shared.EventBus.DependencyInjections.Extensions;
using EShop.Shared.EventBus.DependencyInjections.Options;
using EShop.Shared.EventBus.PipelineObservers;
using EShop.Shared.JsonApi.Extensions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Repositories;
using MassTransit;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace EShop.Catalog.ReadModels.MongoDb.Bootstrapping;

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

        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

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
            resourceGraphBuilder.Add<Category, string?>();
        });

        //If your API project uses MongoDB only(so not in combination with EF Core),
        //then instead of registering all MongoDB resources and repositories individually, you can use:
        services.AddJsonApiMongoDb();

        services.AddScoped(typeof(IResourceReadRepository<,>), typeof(MongoRepository<,>));
        services.AddScoped(typeof(IResourceWriteRepository<,>), typeof(MongoRepository<,>));
        services.AddScoped(typeof(IResourceRepository<,>), typeof(MongoRepository<,>));

        return services;
    }

    public static IServiceCollection AddMassTransitRabbitMQ(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var massTransitConfiguration = new MasstransitConfiguration();
        configuration.GetSection(nameof(MasstransitConfiguration)).Bind(massTransitConfiguration);

        var messageBusOptions = new MessageBusOptions();
        configuration.GetSection(nameof(MessageBusOptions)).Bind(messageBusOptions);

        services.AddMassTransit(cfg =>
        {
            cfg.SetKebabCaseEndpointNameFormatter();

            cfg.AddConsumers(AssemblyReference.Assembly);

            cfg.UsingRabbitMq((context, bus) =>
            {
                bus.Host(massTransitConfiguration.Host, massTransitConfiguration.Port, massTransitConfiguration.VHost, h =>
                {
                    h.Username(massTransitConfiguration.Username);
                    h.Password(massTransitConfiguration.Password);
                });

                bus.UseMessageRetry(retry => retry.Incremental(
                    retryLimit: messageBusOptions.RetryLimit,
                    initialInterval: messageBusOptions.InitialInterval,
                    intervalIncrement: messageBusOptions.IntervalIncrement));

                bus.UseNewtonsoftJsonSerializer();
                bus.ConfigureNewtonsoftJsonSerializer(settings =>
                {
                    settings.Converters.Add(new DateOnlyJsonConverter());
                    settings.Converters.Add(new ExpirationDateOnlyJsonConverter());
                    return settings;
                });
                bus.ConfigureNewtonsoftJsonDeserializer(settings =>
                {
                    settings.Converters.Add(new DateOnlyJsonConverter());
                    settings.Converters.Add(new ExpirationDateOnlyJsonConverter());
                    return settings;
                });

                bus.ConnectPublishObserver(new LoggingPublishObserver());
                bus.ConnectSendObserver(new LoggingSendObserver());
                bus.ConnectReceiveObserver(new LoggingReceiveObserver());
                bus.ConnectConsumeObserver(new LoggingConsumeObserver());

                bus.MessageTopology.SetEntityNameFormatter(new KebabCaseEntityNameFormatter());

                bus.ConfigureRecieveEndpoints(context, environment, Program.ApplicationName);
                bus.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static void ConfigureRecieveEndpoints(
        this IRabbitMqBusFactoryConfigurator bus,
        IRegistrationContext context,
        IWebHostEnvironment environment,
        string serviceName)
    {
        bus.ConfigureEventReceiveEndpoint<CategoryCreatedConsumer, CategoryCreated>(context, environment.EnvironmentName, serviceName);
        bus.ConfigureEventReceiveEndpoint<CategoryUpdatedConsumer, CategoryUpdated>(context, environment.EnvironmentName, serviceName);
    }
}