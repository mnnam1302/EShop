using EShop.Catalog.Application.Boostrapping;
using EShop.Catalog.Application.Shared;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Contracts.JsonConverters;
using EShop.Shared.CQRS;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.EventBus.Services;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;
using EShop.Shared.Sequences.DependencyInjections;
using EShop.Testing.JsonApiApplication;
using EShop.Testing.JsonApiApplication.DependencyInjections;
using EShop.Testing.JsonApiApplication.Providers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Catalog.Tests.Setup;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestShared(this IServiceCollection services, IConfiguration configuration, PostgreSqlTestDatabase testDatabase)
    {
        services.AddMediator(Application.AssemblyReference.Assembly);
        services
            .AddPostgreSqlTestDbContext<CatalogDbContext>(testDatabase)
            .AddDbContextEventSourcing<CatalogDbContext>(configuration);

        services.AddMemoryCacheInfrastructure();

        services
            .AddTestTenantFeatures()
            .AddTestUserPermissions();

        return services;
    }

    public static IServiceCollection AddTestBoostrapping(this IServiceCollection services)
    {
        services
            .AddTestAPI()
            .AddMassTransitInMemory()
            .AddTenantAuthenticationProvider()
            .AddTestServiceBootstrapping();

        return services;
    }

    public static IServiceCollection AddTestAPI(this IServiceCollection services)
    {
        services
            .AddCors()
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

        services.AddScoped<IEventBusGateway, EventBusGateway>();

        return services;
    }
}