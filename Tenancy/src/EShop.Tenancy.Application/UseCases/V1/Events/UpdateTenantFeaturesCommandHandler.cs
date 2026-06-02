using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;

namespace EShop.Tenancy.Application.UseCases.V1.Events;

public sealed class UpdateTenantFeaturesCommand : ICommand
{
    public required string TenantId { get; init; }
}

public class UpdateTenantFeaturesCommandHandler : ICommandHandler<UpdateTenantFeaturesCommand>
{
    private readonly ITenantFeaturesCachingService _tenantFeaturesCachingService;

    public UpdateTenantFeaturesCommandHandler(ITenantFeaturesCachingService tenantFeaturesCachingService)
    {
        _tenantFeaturesCachingService = tenantFeaturesCachingService;
    }

    public async Task<Result> HandleAsync(UpdateTenantFeaturesCommand command, CancellationToken cancellationToken)
    {
        await _tenantFeaturesCachingService.RemoveTenantFeatures(command.TenantId, cancellationToken);
        return Result.Success();
    }
}
