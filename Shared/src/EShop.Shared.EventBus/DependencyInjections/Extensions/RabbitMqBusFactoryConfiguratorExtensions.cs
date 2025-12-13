using EShop.Shared.Contracts.Abstractions.MessageBus;
using MassTransit;

namespace EShop.Shared.EventBus.DependencyInjections.Extensions;

public static class RabbitMqBusFactoryConfiguratorExtensions
{
    public static void ConfigureEventReceiveEndpoint<TConsumer, TEvent>(
        this IRabbitMqBusFactoryConfigurator bus,
        IRegistrationContext context,
        string environment,
        string serviceName)
        where TConsumer : class, IConsumer<TEvent>
        where TEvent : class, IEvent
    {
        string normalizedEnvironment = NormalizeEnvironmentName(environment);
        string normalizedServiceName = serviceName.Trim().ToLowerInvariant();

        string sanitizedQueueName = $"eshop.{normalizedEnvironment}.{normalizedServiceName}.{typeof(TEvent).ToKebabCaseString()}";

        bus.ReceiveEndpoint(
            queueName: sanitizedQueueName,
            configureEndpoint: endpoint =>
            {
                endpoint.ConfigureConsumeTopology = false;
                endpoint.Bind<TEvent>();
                endpoint.ConfigureConsumer<TConsumer>(context);
            });
    }

    private static string NormalizeEnvironmentName(string environment)
    {
        return environment.Trim().ToLowerInvariant() switch
        {
            "development" => "dev",
            "staging" => "stag",
            "production" => "prod",
            _ => string.Empty,
        };
    }
}
