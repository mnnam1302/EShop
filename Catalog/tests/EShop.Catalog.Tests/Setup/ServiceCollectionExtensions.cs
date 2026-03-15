using EShop.Catalog.Application.Bootstrapping;
using EShop.Catalog.Application.Shared;
using EShop.Catalog.ReadModels.MongoDb.Models;
using EShop.Catalog.ReadModels.MongoDb.Persistence;
using EShop.Shared.Authentication.Filters;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Contracts.JsonConverters;
using EShop.Shared.CQRS;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.EventBus.DependencyInjections.Extensions;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.Sequences.DependencyInjections;
using EShop.Testing.JsonApiApplication;
using EShop.Testing.JsonApiApplication.DependencyInjections;
using EShop.Testing.JsonApiApplication.EventBus;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Catalog.Tests.Setup;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCatalogTestShared(this IServiceCollection services, IConfiguration configuration, PostgreSqlTestDatabase testDatabase, MongoDbTestDatabase mongoDatabase)
    {
        services.AddMediator(Application.AssemblyReference.Assembly);
        services.AddMediator(ReadModels.MongoDb.AssemblyReference.Assembly);
        services
            .AddPostgreSqlTestDbContext<CatalogDbContext>(testDatabase)
            .AddDbContextEventSourcing<CatalogDbContext>(configuration);

        services.AddMemoryCacheInfrastructure();

        services
            .AddTenantAuthenticationProvider()
            .AddTestTenantFeatures()
            .AddTestUserPermissions();

        services.AddCatalogReadModelTestServices(mongoDatabase);

        return services;
    }

    public static IServiceCollection AddCatalogTestBoostrapping(this IServiceCollection services)
    {
        services
            .AddCatalogTestApiVersioning()
            .AddTestServiceBootstrapping()
            .AddMassTransitInMemory()
            .AddEventBus()
            .AddPostgreSqlIdempotentConsumer<CatalogDbContext>();

        return services;
    }

    public static IServiceCollection AddCatalogTestApiVersioning(this IServiceCollection services)
    {
        services.AddCors()
            .AddCatalogApiVersioning()
            .AddControllers()
            .AddApplicationPart(Application.AssemblyReference.Assembly);

        return services;
    }

    public static IServiceCollection AddMassTransitInMemory(this IServiceCollection services)
    {
        services.AddMassTransit(cfg =>
        {
            cfg.SetKebabCaseEndpointNameFormatter();
            cfg.AddConsumers(Application.AssemblyReference.Assembly);
            cfg.AddConsumers(ReadModels.MongoDb.AssemblyReference.Assembly);

            cfg.UsingInMemory((context, bus) =>
            {
                bus.UseConsumeFilter(typeof(SystemUserContextConsumeFilter<>), context);

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

                bus.ConnectConsumeObserver(context.GetRequiredService<TestConsumeObserver>());

                bus.ReceiveEndpoint("test_queue", configureEndpoint =>
                {
                    configureEndpoint.ConfigureConsumers(context);
                });
            });
        });

        return services;
    }

    public static IServiceCollection AddCatalogReadModelTestServices(this IServiceCollection services, MongoDbTestDatabase mongoDatabase)
    {
        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddMultiTenantScoping();

        services.AddDbContext<CatalogReadDbContext>(options =>
        {
            options.UseMongoDB(mongoDatabase.ConnectionString, mongoDatabase.DatabaseName);
        });

        services.AddScoped<ICategoryReadRepository, CategoryReadRepository>();

        return services;
    }

    public static IServiceCollection AddTestServiceBootstrapping(this IServiceCollection services)
    {
        services.AddTransient<DbInitializer>();

        services.AddSequenceManagement<SequenceRepository>();
        services.AddOptions<CatalogSequenceOptions>()
            .BindConfiguration(CatalogSequenceOptions.SectionName);

        services.AddScoped<IUnitOfWork, EFUnitOfWork<CatalogDbContext>>();

        return services;
    }
}