using System.ComponentModel.DataAnnotations;
using EShop.Order.Domain.Commands;
using EShop.Order.Domain.StateMachines;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.DomainTools.Exceptions;

namespace EShop.Order.Domain.Aggregates;

public class Order : AggregateRoot<Guid>, IDateTracking, IExcludedFromScoping
{
    [MaxLength(ModelConstants.MediumText)]
    public required string BuyerId { get; set; }

    public DateTimeOffset OrderDate { get; set; }

    [MaxLength(ModelConstants.VeryLongText)]
    public string? Description { get; private set; }

    private readonly List<OrderItem> _orderItems = new();
    public virtual IReadOnlyCollection<OrderItem> OrderItems => _orderItems;

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public OrderStateMachine State => new(() => ParseStatusSafely(), AfterStateUpdated);

    [MaxLength(ModelConstants.ShortText)]
    public string Status { get; private set; } = nameof(OrderState.ReservingInventory);

    private OrderState ParseStatusSafely()
    {
        if (!Enum.TryParse<OrderState>(Status, out var state))
        {
            throw new ArgumentException(nameof(Status));
        }

        return state;
    }

    private void AfterStateUpdated(OrderState newState)
    {
        Status = Enum.GetName(newState)
            ?? throw new DomainException("Order", $"Order state {newState} is invalid.");
    }

    public static Order CreateOrder(PlaceOrderCommand command)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            BuyerId = command.BuyerId,
            OrderDate = DateTimeOffset.UtcNow,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        order.AddOrderItems(command.OrderItems);

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

    public void StartPayment()
    {
        State.Fire(OrderAction.StartPayment);
        LastModifiedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Accept()
    {
        State.Fire(OrderAction.Accept);
        LastModifiedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Reject(string reason)
    {
        State.Fire(OrderAction.Reject);
        Description = reason;
        LastModifiedAtUtc = DateTimeOffset.UtcNow;
    }
}
