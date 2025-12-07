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
        string sanitizedQueueName = $"eshop.{environment.Trim().ToLowerInvariant()}.{serviceName.Trim().ToLowerInvariant()}.{typeof(TEvent).ToKebabCaseString()}";

        bus.ReceiveEndpoint(
            queueName: sanitizedQueueName,
            configureEndpoint: endpoint =>
            {
                endpoint.Bind<TEvent>();
                endpoint.ConfigureConsumer<TConsumer>(context);
            });
    }
}
