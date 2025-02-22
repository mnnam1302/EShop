using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Tenancy.Domain.Entities;

public class Tenant : AggregateRoot<string>, IExcludedFromScoping
{
    [MaxLength(ModelConstants.ShortMediumText)]
    [Required]
    public string? Name { get; set; }

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

    public virtual List<TenantFeature>? TenantFeatures { get; set; }
}