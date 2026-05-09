using EShop.Shared.DomainTools.Entities;
using EShop.Shared.ReadModel;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using static JsonApiDotNetCore.Resources.Annotations.AttrCapabilities;

namespace EShop.Catalog.ReadModels.MongoDb.Models;

[Resource(GenerateControllerEndpoints = JsonApiDotNetCore.Controllers.JsonApiEndpoints.Query)]
public sealed partial class Product : Identifiable<string>, IEntityBase<string>, IScoped, IReadModel
{
    [Attr(Capabilities = AllowView)]
    public Guid DocumentId { get; set; }

    public ulong Version { get; set; }

    [Attr(Capabilities = AllowView | AllowFilter)]
    public string Name { get; set; } = string.Empty;

    [Attr(Capabilities = AllowView)]
    public string Description { get; set; } = string.Empty;

    [Attr(Capabilities = AllowView)]
    public string Slug { get; set; } = string.Empty;

    [Attr(Capabilities = AllowView | AllowFilter)]
    public string CategoryId { get; set; } = string.Empty;

    [Attr(Capabilities = AllowView)]
    public string[] Tags { get; set; } = [];

    [Attr(Capabilities = AllowView)]
    public string[] Images { get; set; } = [];

    [Attr(Capabilities = AllowView | AllowFilter)]
    public string State { get; set; } = string.Empty;

    [Attr(Capabilities = AllowView)]
    public string CreatedByUserId { get; set; } = string.Empty;

    [Attr(Capabilities = AllowView)]
    public DateTimeOffset CreatedAtUtc { get; set; }

    [Attr(Capabilities = AllowView)]
    public string? LastModifiedByUserId { get; set; }

    [Attr(Capabilities = AllowView)]
    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    [Attr(Capabilities = AllowView)]
    public string TenantId { get; set; } = string.Empty;

    public string Scope { get; set; } = string.Empty;

    [Attr(Capabilities = AllowView)]
    public List<ProductVariationDimension> VariationDimensions { get; set; } = [];

    [Attr(Capabilities = AllowView)]
    public List<ProductVariant> Variants { get; set; } = [];
}