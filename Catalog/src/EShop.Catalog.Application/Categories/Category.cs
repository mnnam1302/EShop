using EShop.Shared.Contracts.Shared;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using System.ComponentModel.DataAnnotations;

namespace EShop.Catalog.Application.Categories;

public sealed class Category : Aggregate, IScoped
{
    [MaxLength(ModelConstants.MediumText)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.LongText)]
    public string Slug { get; set; } = string.Empty;

    public Guid ParentId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string Scope { get; set; } = string.Empty;
}
