using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Shared.EventBus.Abstractions;

public interface IEventBusGateway
{
    Task PublishAsync<TEvent>(object @event, CancellationToken cancellationToken = default) where TEvent : class, IIntegrationEvent;

    Task PublishAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default);
}