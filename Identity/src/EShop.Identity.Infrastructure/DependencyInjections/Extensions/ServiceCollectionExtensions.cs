using EShop.Identity.Application.Abstractions;
using EShop.Identity.Infrastructure.Authentication;
using EShop.Identity.Infrastructure.Consumers;
using EShop.Identity.Infrastructure.HashServices;
using EShop.Identity.Infrastructure.Producers;
using EShop.Shared.Contracts.Services.Identity.Permissions;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.EventBus.DependencyInjections.Extensions;
using EShop.Shared.EventBus.DependencyInjections.Options;
using EShop.Shared.EventBus.JsonConverters;
using EShop.Shared.EventBus.PipelineObservers;
using EShop.Shared.EventBus.Services;
using EShop.Shared.Scoping.ResourceAccessControl;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Identity.Infrastructure.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        string serviceName = "identity")
    {
        services.AddOwnerServices();

        services.AddMassTransitRabbitMQ(configuration, environment, serviceName);
        services.AddEventBusGateway();
        services.AddRegistrationFeatures();
        services.AddRegistrationPermissions();

        return services;
    }

    private static void AddOwnerServices(this IServiceCollection services)
    {
        services.AddTransient<IPasswordHasher, PasswordHasher>();
        services.AddTransient<ITokenService, TokenService>();
    }

    private static IServiceCollection AddEventBusGateway(this IServiceCollection services)
    {
        services.AddScoped<IEventBusGateway, EventBusGateway>();
        return services;
    }

    private static IServiceCollection AddRegistrationFeatures(this IServiceCollection services)
    {
        services.AddScoped<IFeatureRegistrationService, UserFeatureRegistrationService>();
        return services;
    }

    private static IServiceCollection AddRegistrationPermissions(this IServiceCollection services)
    {
        services.AddScoped<IPermissionRegistrationService, UserPermissionRegistration>();
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

                bus.ConfigureReceiveEndpoints(context, environment, serviceName);
                bus.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    public static void ConfigureReceiveEndpoints(
        this IRabbitMqBusFactoryConfigurator bus,
        IRegistrationContext context,
        IWebHostEnvironment environment,
        string serviceName)
    {
        bus.ConfigureEventReceiveEndpoint<TenantConsumers.TenantCreatedConsumer, ITenantCreated>(context, environment.EnvironmentName, serviceName);
        bus.ConfigureEventReceiveEndpoint<SupportedPermissionsUpdatedConsumer, SupportedPermissionsUpdated>(context, environment.EnvironmentName, serviceName);
    }
}