namespace EShop.Catalog.Application.Categories.Publish;

public sealed class CategoryPublishedEvent : CategoryDomainEvent
{
    public DateTimeOffset PublishedAtUtc { get; set; }
}