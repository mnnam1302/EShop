namespace EShop.Catalog.Application.Products.AddVariant;

public sealed class VariantCreatedEvent : ProductDomainEvent
{
    public Guid VariantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal DiscountPrice { get; init; }
    public List<VariantDimensionValue> VariantDimensionValues { get; init; } = [];
    public bool IsDefault { get; init; }
}