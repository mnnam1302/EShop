namespace EShop.Shared.Contracts.Services.Tenancy.Tenants;

public sealed class TenantCreated : TenancyEvent
{
    public required string TenantName { get; init; }

    public required string OwnerUsername { get; init; }

    public required string OwnerDisplayName { get; init;  }
    public required string OwnerEmail { get; init;  }
}