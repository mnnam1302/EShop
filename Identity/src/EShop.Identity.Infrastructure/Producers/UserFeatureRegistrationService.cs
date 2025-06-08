using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.EventBus.Services;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Identity.Infrastructure.Producers;

public class UserFeatureRegistrationService(IEventBusGateway eventBusGateway) : IFeatureRegistrationService
{
    private readonly string ApplicationName = "Identity";
    private static readonly IdentityFeature[] features =
    [
        new()
        {
            Id = FeatureConstants.Identity_UserInvites_FeatureId,
            Name = "User Invites",
            Description = "User Invites"
        },
        new()
        {
            Id = FeatureConstants.Identity_OrganisationRingFencing_FeatureId,
            Name = "Organisation Ring Fencing",
            Description = "Organisation Ring Fencing"
        },
        new()
        {
            Id = FeatureConstants.Identity_CustomRoles_FeatureId,
            Name = "Custom Roles",
            Description = "Custom Roles"
        },
        new()
        {
            Id = FeatureConstants.Identity_ExternalApplicationIntegration_FeatureId,
            Name = "External Application Integration",
            Description = "External Application Integration"
        }
    ];

    public async Task RegisterFeatures()
    {
        await eventBusGateway.PublishAsync<ISupportedFeaturesUpdated>(new
        {
            SourceSystemReference = ApplicationName,
            Features = features,
            Action = SupportedFeaturesAction.AddOrUpdate,
            TenantId = string.Empty,
            ActionUserId = string.Empty,
            ActionUserType = string.Empty
        });
    }

    private sealed class IdentityFeature : IFeature
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public string Module => nameof(FeatureModules.EShop_Identity);
        public string State => nameof(FeatureState.Disabled);
    }
}