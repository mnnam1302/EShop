namespace EShop.Catalog.Application.Products.Create;

public sealed class VariantAddedEvent : ProductDomainEvent
{
    public Guid VariantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public double Price { get; set; }
    public double DiscountPrice { get; set; }
    public List<VariantDimensionValue> VariantDimensionValues { get; set; } = [];
    public bool IsDefault { get; set; }
}