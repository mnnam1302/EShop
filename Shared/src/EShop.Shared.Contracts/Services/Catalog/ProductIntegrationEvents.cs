namespace EShop.Shared.Contracts.Services.Catalog;

public sealed class ProductCreated : CatalogIntegrationEvent
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string[] Tags { get; set; } = [];
    public string Slug { get; set; } = string.Empty;
    public string[] Images { get; set; } = [];
    public Guid[] Groups { get; set; } = [];
}