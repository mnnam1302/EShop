using EShop.Catalog.SyncService.MongoDb.Infrastructure.Attributes;

namespace EShop.Catalog.SyncService.MongoDb.Models;

[MongoCollection("Category")]
public sealed class CategoryProjection : Document
{
    public string Name { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public string TenantId { get; set; } = string.Empty;
}