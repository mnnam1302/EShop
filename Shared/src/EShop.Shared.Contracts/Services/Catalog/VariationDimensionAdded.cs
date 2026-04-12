namespace EShop.Shared.Contracts.Services.Catalog;

public sealed class VariationDimensionAdded : CatalogIntegrationEvent
{
    public required Guid ProductId { get; init; }
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public required string[] Values { get; init; }
    public required string DisplayStyle { get; init; }
}
