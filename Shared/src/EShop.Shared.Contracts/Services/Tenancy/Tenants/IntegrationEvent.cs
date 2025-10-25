namespace EShop.Shared.Contracts.Services.Tenancy.Tenants;

public interface ITenantCreated : TenancyEvent
{
    string TenantName { get; }

    string OwnerUsername { get; }

    string OwnerDisplayName { get; }

    string OwnerEmail { get; }
}