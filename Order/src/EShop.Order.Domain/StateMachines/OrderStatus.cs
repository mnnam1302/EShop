namespace EShop.Order.Domain.StateMachines;

public enum OrderStatus
{
    Pending,
    Rejected,
    Accepted,
    Cancelled,
}
