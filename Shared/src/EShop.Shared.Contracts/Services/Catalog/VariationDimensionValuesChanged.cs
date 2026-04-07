namespace EShop.Shared.Contracts.Services.Catalog;

public sealed class VariationDimensionValuesChanged : CatalogIntegrationEvent
{
    public Guid ProductId { get; init; }
    public string DimensionName { get; init; } = string.Empty;
    public string[] Values { get; init; } = [];
}
