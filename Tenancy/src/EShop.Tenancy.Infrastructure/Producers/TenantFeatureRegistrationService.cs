using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.EventBus.Services;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Tenancy.Infrastructure.Producers;

public class TenantFeatureRegistrationService(IEventBusGateway eventBusGateway) : IFeatureRegistrationService
{
    private readonly string ApplicationName = nameof(FeatureModules.EShop_Tenancy);

    private static readonly IEnumerable<IFeature> features = new List<TenancyFeature>
    {
         new TenancyFeature(
             FeatureConstants.Tenancy_SystemFormatConfiguration_FeatureId,
             "System Format Configuration",
             "System Format Configuration",
             FeatureState.Enabled),
    };

    public async Task RegisterFeatures()
    {
        await eventBusGateway.PublishAsync<ISupportedFeaturesUpdated>(new
        {
            SourceSystemReference = ApplicationName,
            Features = features,
            Action = SupportedFeaturesAction.AddOrUpdate,
            TenantId = string.Empty,
            ActionUserId = string.Empty,
            ActionUserType = string.Empty,
        });
    }

    private sealed class TenancyFeature : IFeature
    {
        public TenancyFeature(string id, string name, string description, FeatureState state = FeatureState.Disabled)
        {
            Id = id;
            Name = name;
            Description = description;
            Module = nameof(FeatureModules.EShop_Tenancy);
            State = state.ToString();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Module { get; set; }
        public string State { get; set; }
    }
}