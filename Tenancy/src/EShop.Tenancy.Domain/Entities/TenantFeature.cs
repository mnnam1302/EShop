using EShop.Shared.DomainTools.Entities;
using EShop.Tenancy.Domain.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace EShop.Tenancy.Domain.Entities;

public class TenantFeature : EntityBase<Guid>, IScoped, IUserTracking, IDateTracking
{
    // Using EF Core
    public TenantFeature() { }

    internal TenantFeature(Guid id, string tenantId, string featureId, string state, string scope, string createdBy)
    {
        Id = id;
        TenantId = tenantId;
        FeatureId = featureId;
        State = state;
        Scope = scope;
        CreatedByUserId = createdBy;
        CreatedAtUtc = DateTime.UtcNow;
    }

    [MaxLength(ModelConstants.ShortText)]
    public string FeatureId { get; private set; } = string.Empty;

    public virtual Feature? Feature { get; set; }

    [MaxLength(ModelConstants.TinyText)]
    public string State { get; private set; } = nameof(StateFeature.Enabled);

    [MaxLength(ModelConstants.MediumText)]
    public string CreatedByUserId { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public string? LastModifiedByUserId { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; private set; } = string.Empty;

    public virtual Tenant? Tenant { get; set; }

    [MaxLength(ModelConstants.VeryLongText)]
    public string Scope { get; private set; } = string.Empty;
}