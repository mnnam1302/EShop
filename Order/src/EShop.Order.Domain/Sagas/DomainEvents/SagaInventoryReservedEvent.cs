namespace EShop.Order.Domain.Sagas.DomainEvents;

public sealed class SagaInventoryReservedEvent : OrderSagaDomainEvent
{
    public required Guid ReservationId { get; init; }
}
