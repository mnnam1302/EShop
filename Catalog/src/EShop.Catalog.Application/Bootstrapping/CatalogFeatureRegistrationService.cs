using EShop.Shared.Authentication;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.EventBus.Abstractions;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Catalog.Application.Bootstrapping;

public sealed class CatalogFeatureRegistrationService(IEventBus eventBus) : IFeatureRegistrationService
{
    private static readonly CatalogFeature[] Features =
    [
        new CatalogFeature()
        {
            Id = FeatureConstants.Catalog.Product_FeatureId,
            Name = "Product Builder",
            Description = "Product Builder"
        }
    ];

    public async Task RegisterFeatures()
    {
        await eventBus.PublishAsync(new SupportedFeaturesUpdated
        {
            SourceSystemReference = Program.ApplicationName,
            Features = Features,
            Action = SupportedFeaturesAction.AddOrUpdate,
            TenantId = UserData.SystemTenantId,
            ActionUserId = UserData.SystemUsername,
            ActionUserType = UserTypes.SystemUsers
        });
    }

    private sealed class CatalogFeature : IFeature
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public string State { get; init; } = nameof(FeatureState.Enabled);
        public string Module { get; init; } = nameof(FeatureModules.EShop_Catalog);
    }
}