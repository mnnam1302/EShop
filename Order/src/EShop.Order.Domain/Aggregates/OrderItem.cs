using EShop.Shared.DomainTools.Entities;

namespace EShop.Order.Domain.Aggregates;

public class OrderItem : EntityBase<Guid>, IExcludedFromScoping
{
    public Guid OrderId { get; private set; }
    public Guid VariantId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal? Discount { get; private set; }

    public decimal TotalPrice => Quantity * UnitPrice;

    internal OrderItem(Guid id, Guid orderId, Guid variantId, int quantity, decimal unitPrice, decimal? dicount)
    {
        Id = id;
        OrderId = orderId;
        VariantId = variantId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Discount = dicount;
    }
}
