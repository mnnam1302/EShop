namespace EShop.Catalog.Application.Products.AddVariant;

public sealed class AddVariantRequest
{
    public required string Name { get; init; } = string.Empty;
    public required string Sku { get; init; } = string.Empty;
    public required decimal Price { get; init; }
    public required decimal DiscountPrice { get; init; }
    public required List<AddVariantDimensionValueRequest> DimensionValues { get; init; } = [];
}

public sealed class AddVariantDimensionValueRequest
{
    public required string Name { get; init; } = string.Empty;
    public required string Value { get; init; } = string.Empty;
}