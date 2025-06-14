namespace EShop.Shared.Contracts.Services.Tenancy.Tenants;

public interface ITenantCreated : TenancyEvent
{
    string TenantName { get; }

    string OwnerUsername { get; }

    string OwnerDisplayName { get; }

    string OwnerEmail { get; }
}

public interface ITenantSettingCreated : TenancyEvent
{
    string TenantName { get; }
    string DisplayDateFormat { get; }
    string DisplayTimeFormat { get; }
    string Currency { get; }
    string CurrencyDisplayFormat { get; }
    string DefaultSystemLanguage { get; }
}