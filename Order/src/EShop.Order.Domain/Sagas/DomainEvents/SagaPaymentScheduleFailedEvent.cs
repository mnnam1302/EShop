namespace EShop.Order.Domain.Sagas.DomainEvents;

public sealed class SagaPaymentScheduleFailedEvent : OrderSagaDomainEvent
{
    public required string Reason { get; init; }
}
