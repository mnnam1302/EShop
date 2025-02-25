using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.Scoping;
using EShop.Tenancy.Domain.Aggregates;
using System.ComponentModel.DataAnnotations;

namespace EShop.Tenancy.Domain.Entities;

public class Tenant : TenantAggregate, IExcludedFromScoping
{
    [MaxLength(ModelConstants.ShortMediumText)]
    [Required]
    public string Name { get; set; }

    [MaxLength(ModelConstants.LongText)]
    public string? Description { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    [Required]
    public string? OwnerUsername { get; set; }

    [MaxLength(ModelConstants.MediumLongText)]
    [EmailAddress]
    [Required]
    public string? Email { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public string? PhoneNumber { get; set; }

    private readonly List<TenantFeature>? _tenantFeatures = new();
    public virtual IReadOnlyCollection<TenantFeature>? TenantFeatures => _tenantFeatures;
}