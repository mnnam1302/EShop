namespace EShop.Catalog.Application.Products.Publish;

public sealed class ProductPublishedEvent : ProductDomainEvent
{
    public required DateTimeOffset PublishedAtUtc { get; init; }
    public required string PublishedByUserId { get; init; }
}
