using EShop.Identity.Application.Abstractions;
using EShop.Identity.Infrastructure.Authentication;
using EShop.Identity.Infrastructure.DependencyInjections.Options;
using EShop.Identity.Infrastructure.HashServices;
using EShop.Identity.Infrastructure.PipelineObservers;
using EShop.Identity.Infrastructure.Producers;
using EShop.Shared.Contracts.JsonConverters;
using EShop.Shared.Scoping.ResourceAccessControl;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace EShop.Identity.Infrastructure.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddServices();
        services.AddRegistrationFeatures();
        services.AddMasstransitRabbitMQ(configuration);

        return services;
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddTransient<IPasswordHasher, PasswordHasher>();
        services.AddTransient<ITokenService, TokenService>();
    }

    private static IServiceCollection AddRegistrationFeatures(this IServiceCollection services)
    {
        services.AddScoped<IFeatureRegistrationService, UserFeatureRegistrationService>();
        return services;
    }

    private static IServiceCollection AddMasstransitRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    {
        var massTransitConfiguration = new MasstransitConfiguration();
        configuration.GetSection(nameof(MasstransitConfiguration)).Bind(massTransitConfiguration);

        var messageBusOptions = new MessageBusOptions();
        configuration.GetSection(nameof(MessageBusOptions)).Bind(messageBusOptions);

        services.AddMassTransit(cfg =>
        {
            cfg.SetKebabCaseEndpointNameFormatter();

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
                    settings.Converters.Add(new TypeNameHandlingConverter(TypeNameHandling.Objects));
                    settings.Converters.Add(new DateOnlyJsonConverter());
                    settings.Converters.Add(new ExpirationDateOnlyJsonConverter());
                    return settings;
                });
                bus.ConfigureNewtonsoftJsonDeserializer(settings =>
                {
                    settings.Converters.Add(new TypeNameHandlingConverter(TypeNameHandling.Objects));
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