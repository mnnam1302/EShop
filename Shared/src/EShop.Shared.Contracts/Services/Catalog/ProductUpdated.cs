namespace EShop.Shared.Contracts.Services.Catalog;

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
