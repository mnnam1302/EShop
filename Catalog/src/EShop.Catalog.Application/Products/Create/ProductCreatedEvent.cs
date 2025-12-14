namespace EShop.Catalog.Application.Products.Create;

public sealed class ProductCreatedEvent : ProductDomainEvent
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string[] Tags { get; set; } = [];
    public string Slug { get; set; } = string.Empty;
    public string[] Images { get; set; } = [];
    public Guid[] Groups { get; set; } = [];
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
}
