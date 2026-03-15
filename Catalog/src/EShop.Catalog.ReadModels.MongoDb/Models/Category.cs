using EShop.Shared.DomainTools.Entities;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using static JsonApiDotNetCore.Resources.Annotations.AttrCapabilities;

namespace EShop.Catalog.ReadModels.MongoDb.Models;

[Resource(GenerateControllerEndpoints = JsonApiEndpoints.Query)]
public sealed class Category : Identifiable<string>, IEntityBase<string>, IScoped
{
    public ulong Version { get; set; }

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

    public string Scope { get; set; } = string.Empty;
}