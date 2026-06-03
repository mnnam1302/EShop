using EShop.Shared.Authentication;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Tenancy.Infrastructure.Producers;

public sealed class TenantFeatureRegistrationService(IEventBus eventBusGateway) : IFeatureRegistrationService
{
    private readonly string ApplicationName = nameof(FeatureModules.EShop_Tenancy);

    private static readonly TenancyFeature[] features =
    [
        new TenancyFeature
        {
            Id = FeatureConstants.Tenancy.SystemFormatConfiguration_FeatureId,
            Name = "System Format Configuration",
            Description = "System Format Configuration",
            State = nameof(FeatureState.Enabled)
        }
    ];

    public async Task RegisterFeatures()
    {
        await eventBusGateway.PublishAsync(new SupportedFeaturesUpdated
        {
            SourceSystemReference = ApplicationName,
            Features = features,
            Action = SupportedFeaturesAction.AddOrUpdate,
            TenantId = UserData.SystemTenantId,
            ActionUserId = UserData.SystemUsername,
            ActionUserType = UserTypes.SystemUsers
        });
    }

    private sealed class TenancyFeature : IFeature
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public string State { get; init; } = nameof(FeatureState.Disabled);
        public string Module { get; init; } = nameof(FeatureModules.EShop_Tenancy);
    }
}