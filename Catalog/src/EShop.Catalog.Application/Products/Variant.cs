using EShop.Shared.DomainTools.Entities;

namespace EShop.Catalog.Application.Products;

public sealed class Variant : IEntityBase<Guid>
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public double Price { get; set; }
    public double DiscountPrice { get; set; }
    public VariantState State { get; set; }
    public List<VariantDimensionValue> VariantDimensionValues { get; set; } = [];
}
