namespace EShop.Order.Domain.Sagas.DomainEvents;

public sealed class SagaPaymentScheduledEvent : OrderSagaDomainEvent
{
    public required Guid AccountId { get; init; }
}
