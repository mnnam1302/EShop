using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Contracts.JsonConverters;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.EventBus.DependencyInjections.Extensions;
using EShop.Shared.EventBus.DependencyInjections.Options;
using EShop.Shared.EventBus.PipelineObservers;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Tenancy.Application.Abstractions;
using EShop.Tenancy.Application.Services;
using EShop.Tenancy.Infrastructure.Consumers;
using EShop.Tenancy.Infrastructure.Jobs;
using EShop.Tenancy.Infrastructure.Producers;
using EShop.Tenancy.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Tenancy.Infrastructure.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTenancyInfrastructure(
        this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment, string serviceName = "tenancy")
    {
        services
            .AddMassTransitRabbitMQ(configuration, environment, serviceName)
            .AddEventBus()
            .AddPostgreSqlIdempotentConsumer<TenancyDbContext>()
            .AddRegistrationFeatures();

        services.AddRedisHealthCheck(configuration)
            .AddRedisCacheInfrastructure(configuration);

        services.AddSystemInitialization();

        return services;
    }

    private static IServiceCollection AddMassTransitRabbitMQ(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        string serviceName)
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

                bus.UseMessageRetry(retry
                    => retry.Incremental(
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

                bus.ConfigureRecieveEndpoints(context, environment, serviceName);
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
        bus.ConfigureEventReceiveEndpoint<SupportedFeaturesUpdatedConsumer, SupportedFeaturesUpdated>(
            context,
            environment.EnvironmentName,
            serviceName);

        bus.ConfigureEventReceiveEndpoint<TenantFeaturesUpdatedConsumer, ITenantFeaturesUpdated>(
            context,
            environment.EnvironmentName,
            serviceName);
    }

    private static void AddRegistrationFeatures(this IServiceCollection services)
    {
        services.AddScoped<IFeatureRegistrationService, TenantFeatureRegistrationService>();
    }

    private static void AddSystemInitialization(this IServiceCollection services)
    {
        services.AddTransient<ISystemInitializationService, SystemInitializationService>();
        services.AddHostedService<SystemInitializationJob>();
    }
}