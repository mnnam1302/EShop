using EShop.Shared.DomainTools.Entities;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;

namespace EShop.Catalog.Application.Products;

public sealed class ProductAggregate : Aggregate, IAuditable, IScoped, IRingFenced
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string[] Tags { get; private set; } = [];
    public string Slug { get; private set; } = string.Empty;
    public string[] Images { get; private set; } = [];
    public Guid[] Groups { get; private set; } = [];
    public ProductStateMachine State { get; private set; } = new();

    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? LastModifiedByUserId { get; set; }
    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public List<Variant> Variants { get; private set; } = [];
    public List<VariationDimension> VariationDimensions { get; private set; } = [];

    public required string TenantId { get; set; }
    public required string Scope { get; set; }
}