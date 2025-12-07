using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.EventBus.Abstractions;
using MassTransit;

namespace EShop.Shared.EventBus.Services;

public sealed class EventBusGateway(IPublishEndpoint publishEndpoint) : IEventBusGateway
{
    public async Task PublishAsync<TEvent>(object @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent
    {
        await publishEndpoint.Publish<TEvent>(@event, cancellationToken);
    }
}