using EShop.Order.Domain.Commands;
using EShop.Order.Domain.StateMachines;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.Entities;
using System.ComponentModel.DataAnnotations;

namespace EShop.Order.Domain.Aggregates;

public class Order : AggregateRoot<Guid>, IDateTracking, IExcludedFromScoping
{
    [MaxLength(ModelConstants.MediumText)]
    public string BuyerId { get; set; }

    public DateTimeOffset OrderDate { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string Status { get; private set; } = nameof(OrderStatus.Pending);
    
    [MaxLength(ModelConstants.VeryLongText)]
    public string? Description { get; private set; }

    private List<OrderItem> _orderItems = new();
    public virtual IReadOnlyCollection<OrderItem> OrderItems => _orderItems;

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public static Order CreateOrder(PlaceOrderCommand command)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            BuyerId = command.BuyerId,
            OrderDate = DateTimeOffset.UtcNow,
            Status = nameof(OrderStatus.Pending)
        };

        order.AddOrderItems(command.OrderItems);

        // Raise Domain Event outbox later

        return order;
    }

    public void AddOrderItems(IReadOnlyCollection<OrderItemData> orderItems)
    {
        foreach (var item in orderItems)
        {
            var orderItem = new OrderItem(Guid.NewGuid(), Id, item.VariantId, item.Quantity, item.UnitPrice, item.Discount);
            _orderItems.Add(orderItem);
        }
    }
}
