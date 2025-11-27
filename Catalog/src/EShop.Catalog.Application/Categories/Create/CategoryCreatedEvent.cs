namespace EShop.Catalog.Application.Categories.Create;

public sealed class CategoryCreatedEvent : CategoryEvent
{
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
}
