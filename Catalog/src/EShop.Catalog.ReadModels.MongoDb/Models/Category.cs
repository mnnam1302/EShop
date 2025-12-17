using EShop.Catalog.ReadModels.MongoDb.Infrastructure.Attributes;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources.Annotations;

namespace EShop.Catalog.ReadModels.MongoDb.Models;

[MongoCollection("Category")]
[Resource(GenerateControllerEndpoints = JsonApiEndpoints.Query)]
public sealed class Category : Document
{
    [Attr]
    public string Name { get; set; } = string.Empty;

    [Attr]
    public string Reference { get; set; } = string.Empty;

    [Attr]
    public string Slug { get; set; } = string.Empty;

    [Attr]
    public Guid? ParentId { get; set; }

    [Attr]
    public DateTimeOffset CreatedAtUtc { get; set; }

    [Attr]
    public DateTimeOffset UpdatedAtUtc { get; set; }

    [Attr]
    public string TenantId { get; set; } = string.Empty;
}