using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Order.Domain.Sagas.DomainEvents;

public abstract class OrderSagaDomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTimeOffset TimeStampUtc { get; init; } = DateTimeOffset.UtcNow;
    public ulong Version { get; set; }
}
