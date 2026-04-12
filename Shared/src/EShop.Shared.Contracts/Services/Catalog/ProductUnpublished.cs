namespace EShop.Shared.Contracts.Services.Catalog;

public sealed class ProductUnpublished : CatalogIntegrationEvent
{
    public required Guid ProductId { get; init; }
}
