using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Shared.CQRS.DomainEvent;

public interface IDomainEventHandler<in T> where T : IDomainEvent
{
    Task Handle(T domainEvent, CancellationToken cancellationToken = default);
}
