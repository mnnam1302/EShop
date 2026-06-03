namespace EShop.Shared.Contracts.Abstractions.MessageBus;

public interface IEventBus
{
    Task PublishAsync<TEvent>(object @event, CancellationToken cancellationToken = default) where TEvent : class, IIntegrationEvent;

    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IntegrationEvent;
}
