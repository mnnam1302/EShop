using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Tenancy.Application.Abstrations;

public interface IEventBusGateway
{
    Task PublishAsync<TEvent>(object @event, CancellationToken cancellationToken = default) where TEvent : class, IIntegrationEvent;
}