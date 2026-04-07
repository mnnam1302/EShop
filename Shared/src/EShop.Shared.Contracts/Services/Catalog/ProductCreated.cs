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
    public ProductDefaultVariant DefaultVariant { get; init; } = new();
}

public sealed class ProductDefaultVariant
{
    public Guid VariantId { get; init; }
    public decimal Price { get; init; }
    public decimal DiscountPrice { get; init; }
}
