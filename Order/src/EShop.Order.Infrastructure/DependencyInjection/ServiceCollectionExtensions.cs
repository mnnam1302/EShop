using EShop.Order.Domain.Repositories;
using EShop.Order.Infrastructure.BackgroundJobs;
using EShop.Order.Infrastructure.Repositories;
using EShop.Shared.Authentication.Filters;
using EShop.Shared.Contracts.JsonConverters;
using EShop.Shared.Contracts.Services.Order;
using EShop.Shared.Contracts.Services.Order.Saga;
using EShop.Shared.Diagnostics;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.EventBus.DependencyInjections.Extensions;
using EShop.Shared.EventBus.DependencyInjections.Options;
using EShop.Shared.EventBus.Filters;
using EShop.Shared.EventBus.PipelineObservers;
using EShop.Shared.JsonApi.Extensions;
using Hangfire;
using Hangfire.PostgreSql;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Order.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString(environment);
        services
            .AddPostgreSqlHealthCheck(configuration, connectionString)
            .AddDbContextWithScoping<OrderDbContext>(configuration, connectionString, useRingFencedScoping: false)
            .AddDbContextEventSourcing<OrderDbContext>(configuration)
            .AddRepositoryUnitOfWork();

        return services;
    }

    private static IServiceCollection AddRepositoryUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, EFUnitOfWork<OrderDbContext>>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        return services;
    }

    public static IServiceCollection AddOrderInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddEventBus()
            .AddCommandBus()
            .AddMasstransitRabbitMQ(configuration)
            .AddOrderHangfire(configuration);

        services.AddSingleton(typeof(CorrelationIdLogEnrichFilter<>));
        services.AddScoped<OrderSagaTimeoutJob>();

        return services;
    }

    private static IServiceCollection AddOrderHangfire(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(configuration.GetConnectionStringName());

        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(opt => opt.UseNpgsqlConnection(connectionString)));

        services.AddHangfireServer();

        return services;
    }

    private static IServiceCollection AddMasstransitRabbitMQ(
        this IServiceCollection services,
        IConfiguration configuration)
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
                // SendTopology conventions apply to messages this bus PUBLISHES/SENDS.
                // StocksReserved/StocksNotReserved are published by Inventory — their conventions live there.
                bus.SendTopology.UseCorrelationId<OrderCreated>(x => x.OrderId);
                bus.SendTopology.UseCorrelationId<MakeReservation>(x => x.OrderId);
                bus.SendTopology.UseCorrelationId<ReleaseReservationCommand>(x => x.OrderId);

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
                bus.UseConsumeFilter(typeof(CorrelationIdLogEnrichFilter<>), context);

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
