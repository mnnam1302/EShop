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
        Configure(OrderSagaState.ReservingInventory)
            .Permit(OrderSagaTrigger.InventoryReserved, OrderSagaState.ProcessingPayment)
            .Permit(OrderSagaTrigger.InventoryReservationFailed, OrderSagaState.Failed)
            .Permit(OrderSagaTrigger.Expire, OrderSagaState.Expired);

        Configure(OrderSagaState.ProcessingPayment);

        Configure(OrderSagaState.Failed);

        Configure(OrderSagaState.Expired);
    }
}

public enum OrderSagaState
{
    ReservingInventory,
    ProcessingPayment,
    Failed,
    Expired,
}

public enum OrderSagaTrigger
{
    InventoryReserved,
    InventoryReservationFailed,
    Expire,
}
