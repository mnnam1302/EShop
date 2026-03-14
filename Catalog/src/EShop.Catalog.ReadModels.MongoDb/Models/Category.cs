using EShop.Catalog.ReadModels.MongoDb.Infrastructure.Attributes;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources.Annotations;
using static JsonApiDotNetCore.Resources.Annotations.AttrCapabilities;

namespace EShop.Catalog.ReadModels.MongoDb.Models;

[MongoCollection("Category")]
[Resource(GenerateControllerEndpoints = JsonApiEndpoints.Query)]
public sealed class Category : Document
{
    [Attr(Capabilities = AllowView | AllowFilter)]
    public string Name { get; set; } = string.Empty;

    [Attr(Capabilities = AllowView | AllowFilter)]
    public string Reference { get; set; } = string.Empty;

    [Attr(Capabilities = AllowView)]
    public string Slug { get; set; } = string.Empty;

    [Attr(Capabilities = AllowView)]
    public Guid? ParentId { get; set; }

    [Attr(Capabilities = AllowView)]
    public DateTimeOffset CreatedAtUtc { get; set; }

    [Attr(Capabilities = AllowView)]
    public DateTimeOffset UpdatedAtUtc { get; set; }

    [Attr(Capabilities = AllowView)]
    public string TenantId { get; set; } = string.Empty;
}