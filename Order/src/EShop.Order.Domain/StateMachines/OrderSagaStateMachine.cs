using EShop.Shared.DomainTools.Exceptions;
using Stateless;

namespace EShop.Order.Domain.StateMachines;

public sealed class OrderSagaStateMachine : StateMachine<OrderSagaState, OrderSagaTrigger>
{
    public OrderSagaStateMachine() : base(OrderSagaState.ReservingInventory)
    {
        Configure();
    }

    private void Configure()
    {
        OnUnhandledTrigger((state, trigger) =>
            throw new DomainException("OrderSaga", $"Trigger '{trigger}' is not permitted in state '{state}'."));

        Configure(OrderSagaState.ReservingInventory)
            .Permit(OrderSagaTrigger.InventoryReserved, OrderSagaState.ProcessingPayment)
            .Permit(OrderSagaTrigger.InventoryReservationFailed, OrderSagaState.Failed)
            .Permit(OrderSagaTrigger.Expire, OrderSagaState.Expired);

        Configure(OrderSagaState.ProcessingPayment)
            .Permit(OrderSagaTrigger.PaymentScheduled, OrderSagaState.Completed)
            .Permit(OrderSagaTrigger.PaymentScheduleFailed, OrderSagaState.Failed)
            .Permit(OrderSagaTrigger.Expire, OrderSagaState.Expired);

        Configure(OrderSagaState.Completed);

        Configure(OrderSagaState.Failed);

        Configure(OrderSagaState.Expired);
    }
}

public enum OrderSagaState
{
    ReservingInventory,
    ProcessingPayment,
    Failed,
    Completed,
    Expired,
}

public enum OrderSagaTrigger
{
    InventoryReserved,
    InventoryReservationFailed,
    PaymentScheduled,
    PaymentScheduleFailed,
    Expire,
}
