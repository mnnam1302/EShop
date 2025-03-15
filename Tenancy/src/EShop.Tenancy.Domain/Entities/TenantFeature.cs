using EShop.Shared.DomainTools.Entities;
using EShop.Shared.Scoping;
using EShop.Tenancy.Domain.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace EShop.Tenancy.Domain.Entities;

public class TenantFeature : EntityBase<string>, IScoped, IUserTracking, IDateTracking
{
    internal TenantFeature(string id, string tenantId, string featureId, string state, string? scope, string createdBy)
    {
        Id = id;
        TenantId = tenantId;
        FeatureId = featureId;
        State = state;
        Scope = scope;
        CreatedBy = createdBy;
        CreatedOnUtc = DateTime.UtcNow;
    }

    public void UpdateState(string newState, string? modifiedBy = null)
    {
        if (State != newState)
        {
            State = newState;
            LastModifiedBy = modifiedBy ?? LastModifiedBy;
            LastModifiedOnUtc = DateTime.UtcNow;
        }
    }

    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; set; } = string.Empty;

    public virtual Tenant? Tenant { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string FeatureId { get; set; } = string.Empty;

    public virtual Feature? Feature { get; set; }

    [MaxLength(ModelConstants.TinyText)]
    public string State { get; set; } = StateFeature.Enabled;

    [MaxLength(ModelConstants.LongText)]
    public string? Scope { get; set; }

    public string CreatedBy { get; set; }

    public DateTimeOffset CreatedOnUtc { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTimeOffset? LastModifiedOnUtc { get; set; }
}