namespace EShop.Shared.Contracts.Services.Catalog;

public sealed class VariantPublished : CatalogIntegrationEvent
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
}
