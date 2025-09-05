using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.EventBus.Services;
using EShop.Shared.Scoping.ResourceAccessControl;
using static EShop.Shared.Scoping.ResourceAccessControl.FeatureConstants;

namespace EShop.Configuration.Application.Boostrapping;

public class ConfigurationFeatureRegistrationService(IEventBusGateway eventBusGateway) : IFeatureRegistrationService
{
    private static readonly ConfigurationFeature[] Features =
    [
        new ConfigurationFeature()
        {
            Id = ConfigurationFeatures.ProductBuilder_FeatureId,
            Name = "Product Builder",
            Description =  "Product Builder"
        }
    ];

    public async Task RegisterFeatures()
    {
        await eventBusGateway.PublishAsync<ISupportedFeaturesUpdated>(new
        {
            SourceSystemReference = Program.ApplicationName,
            Features = Features,
            Action = SupportedFeaturesAction.AddOrUpdate,
            TenantId = string.Empty,
            ActionUserId = string.Empty,
            ActionUserType = string.Empty
        });
    }

    private sealed class ConfigurationFeature : IFeature
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public string Module => nameof(FeatureModules.EShop_Configuration);
        public string State => nameof(FeatureState.Enabled);
    }
}