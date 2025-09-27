using EShop.Authorization.Application.Boostrapping;
using EShop.Authorization.Infrastructure;
using EShop.Authorization.Infrastructure.DependencyInjection;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.CQRS;
using EShop.Shared.DomainTools.DependencyInjections;
using EShop.Shared.EventBus.JsonConverters;
using EShop.Testing.JsonApiApplication;
using EShop.Testing.JsonApiApplication.DependencyInjections;
using EShop.Testing.JsonApiApplication.EventBus;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Authorization.Tests.Setups;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestShared(this IServiceCollection services)
    {
        services.AddResiliencePolicy();
        services.AddMediator(Authorization.Application.AssemblyReference.Assembly);

        return services;
    }

    public static IServiceCollection AddTestBoostrapping(this IServiceCollection services, PostgreSqlTestDatabase testDatabase)
    {
        services
            .AddApiServices()
            .AddApplicationServices()
            .AddTestInfrastructure(testDatabase);

        return services;
    }

    private static IServiceCollection AddTestInfrastructure(this IServiceCollection services, PostgreSqlTestDatabase testDatabase)
    {
        services.AddMemoryInfrastructure();

        services.AddPersistence()
            .AddPostgreSqlTestDbContext<AuthorizationDbContext>(testDatabase);

        services.AddEventBus()
            .AddTestMasstransitMemmory();

        return services;
    }

    private static IServiceCollection AddTestMasstransitMemmory(this IServiceCollection services)
    {
        services.AddMassTransit(cfg =>
        {
            cfg.SetKebabCaseEndpointNameFormatter();
            cfg.AddConsumers(Authorization.Infrastructure.AssemblyReference.Assembly);

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
                    configureEndpoint.Observer(new EventObserver<ITenantCreated>(context.GetRequiredService<IIntegrationEventsTracker>()));
                });
            });
        });

        return services;
    }
}
