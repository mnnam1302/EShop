using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.Scoping;
using EShop.Tenancy.Domain.Aggregates;
using EShop.Tenancy.Domain.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace EShop.Tenancy.Domain.Entities;

public class Tenant : TenantAggregate, IExcludedFromScoping
{
    public Tenant() { }

    private Tenant(string id, string name, string ownerUsername, string email, string? phoneNumber, string? description)
    {
        Id = id;
        Name = name;
        OwnerUsername = ownerUsername;
        Email = email;
        PhoneNumber = phoneNumber;
        Description = description;
    }

    public static Tenant Create(Command.CreateTenantCommand command)
    {
        var tenant = new Tenant($"tenant-{Guid.NewGuid()}", command.Name, command.OwnerUsername, command.Email, command.PhoneNumber, command.Description);

        return tenant;
    }

    public void ConfigureFeature(string featureId, string state, string performedBy)
    {
        if (string.IsNullOrEmpty(featureId)) throw new ArgumentNullException(nameof(featureId));

        var existingFeature = _tenantFeatures?.FirstOrDefault(f => f.FeatureId == featureId);
        if (existingFeature != null)
        {
            existingFeature.UpdateState(state, performedBy);
            return;
        }

        var newTenantFeature = new TenantFeature(Guid.NewGuid().ToString(), Id, featureId, state, Id, performedBy);

        _tenantFeatures.Add(newTenantFeature);
    }

    public bool DisableFeature(string featureId)
    {
        if (string.IsNullOrEmpty(featureId)) throw new ArgumentNullException(nameof(featureId));

        var feature = _tenantFeatures?.FirstOrDefault(f => f.FeatureId == featureId);
        if (feature == null) return false;

        return _tenantFeatures.Remove(feature);
    }

    public bool HasFeatureEnabled(string featureId)
    {
        var feature = _tenantFeatures?.FirstOrDefault(f => f.FeatureId == featureId);
        return feature != null && feature.State == StateFeature.Enabled;
    }

    public TenantFeature? GetFeatureConfiguration(string featureId)
    {
        return _tenantFeatures?.FirstOrDefault(f => f.FeatureId == featureId);
    }

    [MaxLength(ModelConstants.ShortMediumText)]
    [Required]
    public string Name { get; private set; } = string.Empty;

    [MaxLength(ModelConstants.LongText)]
    public string? Description { get; private set; }

    [MaxLength(ModelConstants.MediumText)]
    [Required]
    public string? OwnerUsername { get; private set; }

    [MaxLength(ModelConstants.MediumLongText)]
    [EmailAddress]
    [Required]
    public string? Email { get; private set; }

    [MaxLength(ModelConstants.MediumText)]
    public string? PhoneNumber { get; private set; }

    private readonly List<TenantFeature> _tenantFeatures = new();

    public virtual IReadOnlyCollection<TenantFeature> TenantFeatures => _tenantFeatures.AsReadOnly();
}