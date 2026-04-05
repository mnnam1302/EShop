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

public sealed class ProductUpdated : CatalogIntegrationEvent
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public string[] Tags { get; init; } = [];
    public string Slug { get; init; } = string.Empty;
    public string[] Images { get; init; } = [];
    public Guid[] Groups { get; init; } = [];
}

public sealed class ProductPublished : CatalogIntegrationEvent
{
    public required Guid ProductId { get; init; }
}

public sealed class ProductUnpublished : CatalogIntegrationEvent
{
    public required Guid ProductId { get; init; }
}

public sealed class ProductDeleted : CatalogIntegrationEvent
{
    public required Guid ProductId { get; init; }
}

public sealed class VariationDimensionAdded : CatalogIntegrationEvent
{
    public required Guid ProductId { get; init; }
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public required string[] Values { get; init; }
    public required string DisplayStyle { get; init; }
}

public sealed class VariationDimensionUpdated : CatalogIntegrationEvent
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string DisplayStyle { get; init; } = string.Empty;
}

public sealed class VariationDimensionValuesChanged : CatalogIntegrationEvent
{
    public Guid ProductId { get; init; }
    public string DimensionName { get; init; } = string.Empty;
    public string[] Values { get; init; } = [];
}

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

public sealed class VariantUpdated : CatalogIntegrationEvent
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
}

public sealed class VariantPriceChanged : CatalogIntegrationEvent
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
    public decimal OldPrice { get; init; }
    public decimal NewPrice { get; init; }
    public decimal OldDiscountPrice { get; init; }
    public decimal NewDiscountPrice { get; init; }
}

public sealed class VariantPublished : CatalogIntegrationEvent
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
}

public sealed class VariantUnpublished : CatalogIntegrationEvent
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
}