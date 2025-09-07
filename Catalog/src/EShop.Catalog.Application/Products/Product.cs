using EShop.Shared.Contracts.Shared;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Catalog.Application.Products;

public class Product : AggregateRoot<Guid>, IAuditable, IScoped, IRingFenced
{
    [MaxLength(ModelConstants.MediumText)]
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public decimal SalePrice { get; set; }

    public string Specification { get; set; } = string.Empty;

    public int QuantityInStock { get; set; }

    public string ImageUri { get; set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MediumText)]
    public string CreatedByUserId { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MediumText)]
    public string? LastModifiedByUserId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string Scope { get; set; } = string.Empty;
}
