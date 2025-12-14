namespace EShop.Catalog.Application.Categories.Unpublish;

public sealed class CategoryUnpublishedEvent : CategoryDomainEvent
{
    public DateTimeOffset UnpublishedAtUtc { get; set; }
}