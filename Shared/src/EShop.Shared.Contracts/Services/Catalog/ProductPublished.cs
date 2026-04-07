namespace EShop.Shared.Contracts.Services.Catalog;

public sealed class ProductPublished : CatalogIntegrationEvent
{
    public required Guid ProductId { get; init; }
}
