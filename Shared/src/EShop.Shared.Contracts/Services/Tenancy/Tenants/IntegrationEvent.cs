using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Shared.Contracts.Services.Tenancy.Tenants;

public interface TenantCreated : IIntegrationEvent
{
    string TenantName { get; }
    string OwnerUsername { get; }
    string OwnerDisplayName { get; }
    string OwnerEmail { get; }
}