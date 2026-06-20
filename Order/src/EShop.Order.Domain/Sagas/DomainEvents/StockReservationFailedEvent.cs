namespace EShop.Order.Domain.Sagas.DomainEvents;

public sealed class StockReservationFailedEvent : OrderSagaDomainEvent
{
    public required Guid OrderSagaId { get; set; }
    public required string BuyerId { get; init; }
    public required Guid OrderId { get; init; }
    public required string TenantId { get; init; }
    public required string Scope { get; init; }
    public required string FailureReason { get; init; }
}
