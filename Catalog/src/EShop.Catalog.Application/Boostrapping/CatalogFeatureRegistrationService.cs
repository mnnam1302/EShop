using EShop.Shared.Authentication;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.EventBus.Services;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using static EShop.Shared.Scoping.ResourceAccessControl.FeatureIds;

namespace EShop.Catalog.Application.Boostrapping;

public sealed class CatalogFeatureRegistrationService : IFeatureRegistrationService
{
    private readonly IEventBusGateway eventBus;

    public CatalogFeatureRegistrationService(IEventBusGateway eventBus)
    {
        this.eventBus = eventBus;
    }

    private static readonly CatalogFeature[] Features =
    [
        new CatalogFeature()
        {
            Id = FeatureIds.Catalog.ProductBuilder_FeatureId,
            Name = "Product Builder",
            Description =  "Product Builder"
        }
    ];

    public async Task RegisterFeatures()
    {
        await eventBus.PublishAsync<ISupportedFeaturesUpdated>(new
        {
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
        public string Module => nameof(FeatureModules.EShop_Configuration);
        public string State => nameof(FeatureState.Enabled);
    }
}
