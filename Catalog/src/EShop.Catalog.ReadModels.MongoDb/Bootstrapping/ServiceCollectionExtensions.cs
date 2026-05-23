using EShop.Catalog.ReadModels.MongoDb.Bootstrapping;
using EShop.Catalog.ReadModels.MongoDb.Models;
using EShop.Catalog.ReadModels.MongoDb.Persistence;
using EShop.Shared.Authentication.Filters;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Contracts.JsonConverters;
using EShop.Shared.CQRS;
using EShop.Shared.Diagnostics;
using EShop.Shared.EventBus.DependencyInjections.Extensions;
using EShop.Shared.EventBus.DependencyInjections.Options;
using EShop.Shared.EventBus.PipelineObservers;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.ReadModel.EfCore;
using JsonApiDotNetCore.Configuration;
using MassTransit;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EShop.Catalog.ReadModels.MongoDb.Bootstrapping;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddGlobalExceptionMiddleware()
            .AddMediator(AssemblyReference.Assembly);

        services
            .AddRedisHealthCheck(configuration)
            .AddRedisCacheInfrastructure(configuration);

        services
            .AddTenantAuthenticationProvider()
            .AddUserPermissionsProvider()
            .AddUserOrganizationContextProvider()
            .AddTenantFeaturesProvider();

        return services;
    }

    public static IServiceCollection AddBoostrapping(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        services.AddCors()
            .AddSwagger()
            .AddApiVersioning()
            .AddJsonApiDotNet();

        services.AddMongoDbPersistence(configuration);
        services.AddMassTransitRabbitMQ(configuration, webHostEnvironment);

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

    public static IServiceCollection AddMongoDbPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMultiTenantScoping();

        if (configuration.IsRunningInAspire())
        {
            services.AddDbContext<CatalogReadDbContext>((serviceProvider, options) =>
            {
                var connectionString = configuration.GetConnectionString("catalogMongoDatabase")
                    ?? throw new InvalidOperationException("Aspire connection string 'catalogMongoDatabase' not found.");

                var mongoUrl = new MongoUrl(connectionString);
                var databaseName = mongoUrl.DatabaseName ?? "eshop-catalog";

                options.UseMongoDB(connectionString, databaseName);
            });
        }
        else
        {
            services.AddOptions<MongoDbSettings>()
                .Bind(configuration.GetSection(MongoDbSettings.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddDbContext<CatalogReadDbContext>((serviceProvider, options) =>
            {
                var mongoSettings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                options.UseMongoDB(mongoSettings.ConnectionString, mongoSettings.DatabaseName);
            });
        }

        services.AddScoped<ICategoryReadRepository, CategoryReadRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();

        services.UseEfCoreReadModelStore<Product, CatalogReadDbContext>("ProductId");

        return services;
    }

    public static IServiceCollection AddJsonApiDotNet(this IServiceCollection services)
    {
        services.AddJsonApi<CatalogReadDbContext>(options =>
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
            resourceGraphBuilder.Add<Category, string>();
            resourceGraphBuilder.Add<Product, string>();
        });

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
                if (configuration.IsRunningInAspire())
                {
                    var connectionString = configuration.GetConnectionString("rabbitmq");
                    bus.Host(connectionString);
                }
                else
                {
                    bus.Host(massTransitConfiguration.Host, massTransitConfiguration.Port, massTransitConfiguration.VHost, h =>
                    {
                        h.Username(massTransitConfiguration.Username);
                        h.Password(massTransitConfiguration.Password);
                    });
                }

                bus.UseConsumeFilter(typeof(SystemUserContextConsumeFilter<>), context);

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

                bus.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
