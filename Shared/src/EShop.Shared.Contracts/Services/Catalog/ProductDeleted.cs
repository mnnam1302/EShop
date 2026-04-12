namespace EShop.Shared.Contracts.Services.Catalog;

public sealed class ProductDeleted : CatalogIntegrationEvent
{
    public required Guid ProductId { get; init; }
}
