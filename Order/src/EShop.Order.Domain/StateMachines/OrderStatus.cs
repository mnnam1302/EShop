namespace EShop.Order.Domain.StateMachines;

public enum OrderStatus
{
    Pending,
    Rejected,
    Confirmed,
    Cancelled,
}
