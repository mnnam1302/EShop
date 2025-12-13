using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.EventBus.Abstractions;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Tenancy.Infrastructure.Producers;

public sealed class TenantFeatureRegistrationService(IEventBus eventBusGateway) : IFeatureRegistrationService
{
    private readonly string ApplicationName = nameof(FeatureModules.EShop_Tenancy);

    private static readonly IEnumerable<IFeature> features = new List<TenancyFeature>
    {
         new TenancyFeature(
             FeatureConstants.Tenancy.SystemFormatConfiguration_FeatureId,
             "System Format Configuration",
             "System Format Configuration",
             FeatureState.Enabled),
    };

    public async Task RegisterFeatures()
    {
        await eventBusGateway.PublishAsync<SupportedFeaturesUpdated>(new
        {
            EventId = Guid.NewGuid(),
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
            State = state.ToString();
            Module = nameof(FeatureModules.EShop_Tenancy);
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public string Module { get; set; }
    }
}