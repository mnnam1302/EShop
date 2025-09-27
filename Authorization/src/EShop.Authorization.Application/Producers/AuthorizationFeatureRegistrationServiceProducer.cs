using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.EventBus.Services;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Authorization.Application.Producers;

public sealed class AuthorizationFeatureRegistrationServiceProducer
{
    private static readonly AuthorizationFeature[] features =
    [
        new()
        {
            Id = FeatureIds.Authorization.OrganisationRingFencing_FeatureId,
            Name = "Organisation Ring Fencing",
            Description = "Organisation Ring Fencing"
        },
        new()
        {
            Id = FeatureIds.Authorization.ExternalApplicationIntegration_FeatureId,
            Name = "External Application Integration",
            Description = "External Application Integration"
        },
        new()
        {
            Id = FeatureIds.Authorization.CustomRoles_FeatureId,
            Name = "Custom Roles",
            Description = "Custom Roles"
        },
        new()
        {
            Id = FeatureIds.Authorization.UserInvites_FeatureId,
            Name = "User Invites",
            Description = "User Invites"
        },
    ];

    private readonly IEventBusGateway eventBusGateway;

    public AuthorizationFeatureRegistrationServiceProducer(IEventBusGateway eventBusGateway)
    {
        this.eventBusGateway = eventBusGateway;
    }

    public async Task RegisterFeatures()
    {
        await eventBusGateway.PublishAsync<ISupportedFeaturesUpdated>(new
        {
            SourceSystemReference = Program.ApplicationName,
            Features = features,
            Action = SupportedFeaturesAction.AddOrUpdate,
            TenantId = string.Empty,
            ActionUserId = string.Empty,
            ActionUserType = string.Empty
        });
    }

    private sealed class AuthorizationFeature : IFeature
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public string Module => nameof(FeatureModules.EShop_Authorization);
        public string State => nameof(FeatureState.Disabled);
    }
}
