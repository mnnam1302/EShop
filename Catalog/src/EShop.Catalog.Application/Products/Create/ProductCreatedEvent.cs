namespace EShop.Catalog.Application.Products.Create;

public sealed class ProductCreatedEvent : ProductDomainEvent
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public string[] Tags { get; init; } = [];
    public string Slug { get; init; } = string.Empty;
    public string[] Images { get; init; } = [];
    public Guid[] Groups { get; init; } = [];
    public DateTimeOffset CreatedAtUtc { get; init; }
    public string CreatedByUserId { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Scope { get; init; } = string.Empty;
}