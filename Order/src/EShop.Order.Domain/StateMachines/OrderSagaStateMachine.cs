namespace EShop.Order.Domain.StateMachines;

public sealed class OrderSagaStateMachine
{
}

public enum OrderSagaStates
{
    AwaitingStockReservation,
    StocksAccepted,
    StocksRejected,
}
