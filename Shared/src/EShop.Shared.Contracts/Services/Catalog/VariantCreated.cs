namespace EShop.Shared.Contracts.Services.Catalog;

public sealed class VariantCreated : CatalogIntegrationEvent
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal DiscountPrice { get; init; }
    public bool IsDefault { get; init; }
    public List<VariantDimensionValueDto> VariantDimensionValues { get; init; } = [];
}

public sealed class VariantDimensionValueDto
{
    public string Name { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}