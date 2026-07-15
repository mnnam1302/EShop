using EShop.Shared.Contracts.Abstractions.Mediator;

namespace EShop.Tenancy.Application.UseCases.Tenants.ClearTenantFeatures;

public sealed class ClearTenantFeaturesCommand : ICommand
{
    public required string TenantId { get; init; }
}
