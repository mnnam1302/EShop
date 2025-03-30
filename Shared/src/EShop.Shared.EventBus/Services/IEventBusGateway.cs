using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Shared.EventBus.Services;

public interface IEventBusGateway
{
    Task PublishAsync<TEvent>(object @event, CancellationToken cancellationToken = default) where TEvent : class, IIntegrationEvent;
}