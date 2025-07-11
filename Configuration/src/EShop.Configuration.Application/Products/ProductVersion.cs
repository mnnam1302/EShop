using EShop.Shared.Contracts.Shared;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Configuration.Application.Products;

public class ProductVersion : EntityBase<Guid>, IScoped
{
    [MaxLength(ModelConstants.ShortMediumText)]
    public string Reference { get; set; } = string.Empty;

    public int Version { get; set; }

    [MaxLength(ModelConstants.LongText)]
    public string? Description { get; set; }

    public Guid ProductId { get; set; }

    public virtual Product Product { get; set; } = null!;

    public ProductDefinition ProductDefinition { get; set; } = new ProductDefinition();

    public Guid? ProductLookupId { get; set; }

    public virtual Lookup? ProductLookup { get; set; }

    public bool IsPublished { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public string? PublishedByUserId { get; set; } = string.Empty;

    public DateTimeOffset? PublishedAtUtc { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public string? LastModifiedByUserId { get; set; } = string.Empty;

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; private set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string Scope { get; private set; } = string.Empty;
}