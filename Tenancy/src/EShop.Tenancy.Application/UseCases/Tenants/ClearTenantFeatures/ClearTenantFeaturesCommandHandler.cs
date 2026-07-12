using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;

namespace EShop.Tenancy.Application.UseCases.Tenants.ClearTenantFeatures;

public class ClearTenantFeaturesCommandHandler : ICommandHandler<ClearTenantFeaturesCommand>
{
    private readonly ITenantFeaturesCachingService _tenantFeaturesCachingService;

    public ClearTenantFeaturesCommandHandler(ITenantFeaturesCachingService tenantFeaturesCachingService)
    {
        _tenantFeaturesCachingService = tenantFeaturesCachingService;
    }

    public async Task<Result> HandleAsync(ClearTenantFeaturesCommand command, CancellationToken cancellationToken)
    {
        await _tenantFeaturesCachingService.RemoveTenantFeatures(command.TenantId, cancellationToken);
        return Result.Success();
    }
}
