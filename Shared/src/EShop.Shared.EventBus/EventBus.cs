using EShop.Shared.Contracts.Abstractions.MessageBus;
using MassTransit;

namespace EShop.Shared.EventBus;

public sealed class EventBus(IPublishEndpoint publishEndpoint) : IEventBus
{
    public async Task PublishAsync<TEvent>(object @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent
    {
        await publishEndpoint.Publish<TEvent>(@event, cancellationToken);
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        await publishEndpoint.Publish(@event, cancellationToken);
    }
}