namespace EShop.Order.Domain.StateMachines;

public enum OrderStatus
{
    Pending,
    Created,
    Rejected,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
}
