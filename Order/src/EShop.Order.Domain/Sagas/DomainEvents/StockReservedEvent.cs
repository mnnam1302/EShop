namespace EShop.Order.Domain.Sagas.DomainEvents;

public sealed class StockReservedEvent : OrderSagaDomainEvent
{
    public required Guid OrderId { get; init; }
    public required Guid ReservationId { get; init; }
}
