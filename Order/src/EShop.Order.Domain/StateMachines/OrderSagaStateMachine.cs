using Stateless;

namespace EShop.Order.Domain.StateMachines;

public sealed class OrderSagaStateMachine : StateMachine<OrderSagaState, OrderSagaTrigger>
{
    public OrderSagaStateMachine() : base(OrderSagaState.AwaitingStockReservation)
    {
        Configure();
    }

    private void Configure()
    {
        Configure(OrderSagaState.AwaitingStockReservation)
            .Permit(OrderSagaTrigger.StocksReserved, OrderSagaState.StocksAccepted)
            .Permit(OrderSagaTrigger.StocksNotReserved, OrderSagaState.StocksRejected)
            .Permit(OrderSagaTrigger.Timeout, OrderSagaState.TimedOut);

        Configure(OrderSagaState.StocksAccepted);
        Configure(OrderSagaState.StocksRejected);
        Configure(OrderSagaState.TimedOut);
    }
}

public enum OrderSagaState
{
    AwaitingStockReservation,
    StocksAccepted,
    StocksRejected,
    TimedOut,
}

public enum OrderSagaTrigger
{
    StocksReserved,
    StocksNotReserved,
    Timeout,
}
