namespace EShop.Order.Domain.StateMachines;

public sealed class OrderSagaStateMachine
{
}

public static class OrderSagaStates
{
    public const string Initial = "Initial";
    public const string AwaitingStockReservation = "AwaitingStockReservation";
    public const string AwaitingOrderPersistence = "AwaitingOrderPersistence";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
}
