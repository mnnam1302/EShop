namespace EShop.Shared.Contracts.Services.Catalog;

public sealed class VariationDimensionUpdated : CatalogIntegrationEvent
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string DisplayStyle { get; init; } = string.Empty;
}
