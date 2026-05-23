using EShop.Shared.DomainTools.Entities;

namespace EShop.Order.Domain.Aggregates;

public sealed class OrderItem : EntityBase<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal? Discount { get; private set; }

    public decimal TotalPrice => Quantity * UnitPrice;

    internal OrderItem(Guid id, Guid orderId, Guid productId, int quantity, decimal unitPrice, decimal? dicount)
    {
        Id = id;
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Discount = dicount;
    }
}
