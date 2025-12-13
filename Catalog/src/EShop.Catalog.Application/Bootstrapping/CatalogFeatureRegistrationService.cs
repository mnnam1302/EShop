using EShop.Shared.Authentication;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.EventBus.Abstractions;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Catalog.Application.Bootstrapping;

public sealed class CatalogFeatureRegistrationService(IEventBusGateway eventBus) : IFeatureRegistrationService
{
    private static readonly CatalogFeature[] Features =
    [
        new CatalogFeature()
        {
            Id = FeatureConstants.Catalog.ProductBuilder_FeatureId,
            Name = "Product Builder",
            Description = "Product Builder"
        }
    ];

    public async Task RegisterFeatures()
    {
        await eventBus.PublishAsync<SupportedFeaturesUpdated>(new
        {
            EventId = Guid.NewGuid(),
            SourceSystemReference = Program.ApplicationName,
            Features = Features,
            Action = SupportedFeaturesAction.AddOrUpdate,
            TenantId = string.Empty,
            ActionUserId = UserData.SystemUsername,
            ActionUserType = string.Empty
        });
    }

    private sealed class CatalogFeature : IFeature
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public string Module => nameof(FeatureModules.EShop_Catalog);
        public string State => nameof(FeatureState.Enabled);
    }
}