namespace EShop.Catalog.Application.Products.PublishVariant;

public sealed class VariantPublishedEvent : ProductDomainEvent
{
    public required Guid VariantId { get; init; }
    public required DateTimeOffset PublishedAtUtc { get; init; }
    public required string PublishedByUserId { get; init; }
}