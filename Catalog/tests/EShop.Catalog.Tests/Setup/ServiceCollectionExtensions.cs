using EShop.Catalog.Application.Bootstrapping;
using EShop.Catalog.Application.Shared;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Catalog.Tests.Setup;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCatalogTestShared(this IServiceCollection services, IConfiguration configuration, PostgreSqlTestDatabase testDatabase)
    {
        services.AddMediator(Application.AssemblyReference.Assembly);
        services
            .AddPostgreSqlTestDbContext<CatalogDbContext>(testDatabase)
            .AddDbContextEventSourcing<CatalogDbContext>(configuration);

        services.AddMemoryCacheInfrastructure();

        services
            .AddTenantAuthenticationProvider()
            .AddTestTenantFeatures()
            .AddTestUserPermissions();

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
            .AddSwagger()
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

            cfg.UsingInMemory((context, bus) =>
            {
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