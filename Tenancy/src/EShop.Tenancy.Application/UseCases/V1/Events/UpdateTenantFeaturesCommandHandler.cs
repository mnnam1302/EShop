using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;

namespace EShop.Tenancy.Application.UseCases.V1.Events;

public class UpdateTenantFeaturesCommandHandler : ICommandHandler<Command.UpdateTenantFeaturesCommand>
{
    private readonly ITenantFeaturesCachingService _tenantFeaturesCachingService;

    public UpdateTenantFeaturesCommandHandler(ITenantFeaturesCachingService tenantFeaturesCachingService)
    {
        _tenantFeaturesCachingService = tenantFeaturesCachingService;
    }

    public async Task<Result> Handle(Command.UpdateTenantFeaturesCommand request, CancellationToken cancellationToken)
    {
        await _tenantFeaturesCachingService.RemoveTenantFeatures(request.TenantId, cancellationToken);
        return Result.Success();
    }
}