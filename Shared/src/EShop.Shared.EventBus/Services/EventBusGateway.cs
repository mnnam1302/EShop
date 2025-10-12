using EShop.Shared.Contracts.Abstractions.MessageBus;
using MassTransit;

namespace EShop.Shared.EventBus.Services;

public class EventBusGateway : IEventBusGateway
{
    private readonly IPublishEndpoint _publishEndpoint;

    public EventBusGateway(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAsync<TEvent>(object @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent
    {
        await _publishEndpoint.Publish<TEvent>(@event, cancellationToken);
    }

    public async Task PublishAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        await _publishEndpoint.Publish(@event, cancellationToken);
    }
}