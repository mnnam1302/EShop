namespace EShop.Catalog.Application.Products.UnpublishVariant;

public sealed class VariantUnpublishedEvent : ProductDomainEvent
{
    public required Guid VariantId { get; init; }
    public required DateTimeOffset UnpublishedAtUtc { get; init; }
    public required string UnpublishedByUserId { get; init; }
}
