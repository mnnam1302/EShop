using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.Scoping;
using EShop.Tenancy.Domain.Aggregates;
using EShop.Tenancy.Domain.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace EShop.Tenancy.Domain.Entities;

public class Tenant : TenantAggregate, IExcludedFromScoping
{
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

    // EF Core
    public Tenant() { }

    public Tenant(string id, string name, string ownerUsername, string email, string? phoneNumber, string? description)
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
        EnsureValidTenant(command);

        var tenant = new Tenant(command.Id, command.Name, command.OwnerUsername, command.Email, command.PhoneNumber, command.Description);

        return tenant;
    }

    private static void EnsureValidTenant(Command.CreateTenantCommand command)
    {
        AssertTenantId(command.Id);

        command.Id = command.Id.ToLowerInvariant();
        var tenantId = command.Id;

        if (UserData.IsSystemUser(command.OwnerUsername))
        {
            throw new ArgumentException("Invalid username");
        }

        var usernameWithoutDomainSuffix = RemoveDomainSuffix(command.OwnerUsername, tenantId);
        AssertUsername(usernameWithoutDomainSuffix);

        if (!isFullUsername(command.OwnerUsername, tenantId))
        {
            command.OwnerUsername = $"{command.OwnerUsername}@{tenantId}";
        }
    }

    private static void AssertTenantId(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new ArgumentNullException(nameof(tenantId), "Tenant id cannot be empty");
        }

        if (tenantId.Equals(UserData.EShopSupportGroup, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"{tenantId} cannot be used as tenant ID.", nameof(tenantId));
        }

        if (tenantId.Any(c => !IsAllowedTenantIdCharacter(c)))
        {
            throw new ArgumentException("Tenant ID can only contain letters (a-z), digits, dashes, and underscores");
        }
    }

    private static bool IsAllowedTenantIdCharacter(char c)
    {
        return char.IsLetterOrDigit(c) || c == '-' || c == '_';
    }

    public static string RemoveDomainSuffix(string username, string tenantId) => username.Replace($"@{tenantId}", null, StringComparison.OrdinalIgnoreCase);

    private static void AssertUsername(string usernameWithoutDomainSuffix)
    {
        if (string.IsNullOrWhiteSpace(usernameWithoutDomainSuffix))
        {
            throw new ArgumentException("Username cannot be null or whitespace");
        }

        var invalidCharactersPattern = @"[<>;@&/\\\s]";
        if (System.Text.RegularExpressions.Regex.IsMatch(usernameWithoutDomainSuffix, invalidCharactersPattern))
        {
            throw new ArgumentException("Username cannot contain special characters");
        }
    }

    private static bool isFullUsername(string ownerUsername, string tenantId)
    {
        var domainSuffix = $"@{tenantId}";
        return ownerUsername.Contains(domainSuffix, StringComparison.OrdinalIgnoreCase);
    }

    public void AddTenantFeature(string featureId, string state, string createdBy)
    {
        if (string.IsNullOrEmpty(featureId))
        {
            throw new ArgumentNullException(nameof(featureId));
        }

        var newTenantFeature = new TenantFeature(Guid.NewGuid().ToString(), Id, featureId, state, Id, createdBy);
        _tenantFeatures.Add(newTenantFeature);
    }

    public void RemoveTenantFeature(string featureId)
    {
        var feature = _tenantFeatures.FirstOrDefault(f => f.FeatureId == featureId);
        if (feature is not null)
        {
            _tenantFeatures.Remove(feature);
        }
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
}