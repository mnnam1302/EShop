using EShop.Shared.Authentication;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.DomainTools.Exceptions;
using EShop.Shared.DomainTools.Specifications;
using EShop.Tenancy.Domain.Commands;
using EShop.Tenancy.Domain.RateLimiting;
using EShop.Tenancy.Domain.Specifications;
using System.ComponentModel.DataAnnotations;

namespace EShop.Tenancy.Domain.Entities;

public class Tenant : AggregateRoot<string>, IExcludedFromScoping
{
    [MaxLength(ModelConstants.ShortMediumText)]
    public string Name { get; private set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string? Description { get; private set; }

    [MaxLength(ModelConstants.MediumText)]
    public string? OwnerUsername { get; private set; }

    [EmailAddress]
    [MaxLength(ModelConstants.MediumLongText)]
    public string? Email { get; private set; }

    [MaxLength(ModelConstants.MediumText)]
    public string? PhoneNumber { get; private set; }

    private readonly List<TenantFeature> _tenantFeatures = [];
    public virtual IReadOnlyCollection<TenantFeature> TenantFeatures => _tenantFeatures.AsReadOnly();

    private readonly List<TenantSetting> tenantSettings = [];
    public virtual IReadOnlyCollection<TenantSetting> TenantSettings => tenantSettings.AsReadOnly();

    public static Tenant CreateSystemTenant(string id, string name, string ownerUsername, string ownerEmail)
    {
        var tenant = new Tenant
        {
            Id = id,
            Name = name,
            OwnerUsername = ownerUsername,
            Email = ownerEmail,
            Description = "Tenant for system administration and system user."
        };

        return tenant;
    }

    public static Tenant Create(CreateTenantCommand command)
    {
        AssertTenant(command);

        var tenant = new Tenant
        {
            Id = command.Id,
            Name = command.Name,
            OwnerUsername = command.OwnerUsername,
            Email = command.OwnerEmail,
            PhoneNumber = command.PhoneNumber,
            Description = command.Description
        };

        return tenant;
    }

    private static void AssertTenant(CreateTenantCommand command)
    {
        AssertTenantId(command.Id);

        command.Id = command.Id.ToLowerInvariant();
        var tenantId = command.Id;

        if (UserData.IsSystemUser(command.OwnerUsername))
        {
            throw new BadRequestException("Invalid username");
        }

        var usernameWithoutDomainSuffix = RemoveDomainSuffix(command.OwnerUsername, tenantId);
        AssertUsername(usernameWithoutDomainSuffix);

        if (!IsFullUsername(command.OwnerUsername, tenantId))
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

    public static string RemoveDomainSuffix(string username, string tenantId)
    {
        var suffix = $"@{tenantId}";
        return username.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
            ? username[..^suffix.Length]
            : username;
    }

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

    private static bool IsFullUsername(string ownerUsername, string tenantId)
    {
        return ownerUsername.Contains($"@{tenantId}", StringComparison.OrdinalIgnoreCase);
    }

    public void AddTenantFeature(string featureId, string state, string createdBy)
    {
        if (string.IsNullOrEmpty(featureId))
        {
            throw new BadRequestException("Feature ID must not ne null or empty.");
        }

        var newTenantFeature = new TenantFeature(Guid.NewGuid(), Id, featureId, state, Id, createdBy);
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

    public void AddDefaultTenantSetting()
    {
        var tenantSetting = new TenantSetting
        {
            Id = Guid.NewGuid(),
            DisplayDateFormat = SupportedDateTimeFormats.DefaultDateFormat,
            DisplayTimeFormat = SupportedDateTimeFormats.DefaultTimeFormat,
            DefaultCurrency = SupportedCurrencies.DefaultCurrencyCode,
            CurrencyDisplayFormat = SupportedCurrencies.DefaultCurrencyDisplayFormat,
            DefaultSystemLanguage = SupportedLanguages.DefaultLanguageCode,
            TenantId = Id,
            Scope = Id
        };

        tenantSettings.Add(tenantSetting);
    }

    public void SetRateLimitPolicy(RateLimitPolicy policy)
    {
        RateLimitPolicySpecification.New().ThrowDomainErrorIfNotSatisfied(policy);

        var tenantSetting = tenantSettings.SingleOrDefault();
        if (tenantSetting is null)
        {
            throw new BadRequestException("Tenant settings must be initialized before setting a rate-limit policy.");
        }

        tenantSetting.RateLimitPolicy = policy;
    }
}
