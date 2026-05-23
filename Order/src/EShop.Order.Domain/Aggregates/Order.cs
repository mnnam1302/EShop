using EShop.Order.Domain.StateMachines;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.Entities;
using System.ComponentModel.DataAnnotations;

namespace EShop.Order.Domain.Aggregates;

public class Order : AggregateRoot<Guid>, IDateTracking, IExcludedFromScoping
{
    public DateTimeOffset OrderDate { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string Status { get; private set; } = nameof(OrderStatus.Pending);
    
    [MaxLength(ModelConstants.VeryLongText)]
    public string? Description { get; private set; }

    private List<OrderItem> _orderItems;
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? LastModifiedAtUtc { get; set; }
}
