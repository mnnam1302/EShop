using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Inventory.Domain.DomainEvents;

public abstract class ReservationDomainEvent : IDomainEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTimeOffset TimeStampUtc { get; set; } = DateTimeOffset.UtcNow;
    public ulong Version { get; set; }
}
