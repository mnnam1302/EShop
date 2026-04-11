namespace EShop.Shared.Contracts.Services.Catalog;

public sealed class ProductCreated : CatalogIntegrationEvent
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public string[] Tags { get; init; } = [];
    public string Slug { get; init; } = string.Empty;
    public string[] Images { get; init; } = [];
    public Guid[] Groups { get; init; } = [];
    public required ProductDefaultVariant DefaultVariant { get; init; }
}

public sealed class ProductDefaultVariant
{
    public required Guid VariantId { get; init; }
    public required string Name { get; init; }
    public required string Sku { get; init; }
    public required decimal Price { get; init; }
    public required decimal DiscountPrice { get; init; }
    public required bool IsDefault { get; init; }
    public required string State { get; init; }
}
