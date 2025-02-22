using EShop.Shared.DomainTools.Entities;
using EShop.Shared.Scoping;
using EShop.Tenancy.Domain.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace EShop.Tenancy.Domain.Entities;

public class TenantFeature : EntityBase<string>, IScoped
{
    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; set; }
    public virtual Tenant Tenant { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string FeatureId { get; set; }

    [MaxLength(ModelConstants.TinyText)]
    public string State { get; set; } = StateFeature.Enabled;

    [MaxLength(ModelConstants.LongText)]
    public string? Scope { get; set; }
}