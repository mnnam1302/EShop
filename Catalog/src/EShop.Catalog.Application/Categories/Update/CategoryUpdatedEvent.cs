namespace EShop.Catalog.Application.Categories.Update;

public class CategoryUpdatedEvent : CategoryDomainEvent
{
    public required string Name { get; set; }
    public required string Reference { get; set; }
    public required string Slug { get; set; }
    public Guid? ParentId { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}