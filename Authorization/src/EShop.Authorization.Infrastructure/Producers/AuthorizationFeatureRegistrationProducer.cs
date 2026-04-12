using EShop.Shared.Authentication;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.EventBus.Abstractions;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Authorization.Infrastructure.Producers;

internal sealed class AuthorizationFeatureRegistrationProducer : IFeatureRegistrationService
{
    private readonly string ApplicationName = "Authorization";

    private static readonly AuthorizationFeature[] features =
    [
        new()
        {
            Id = FeatureConstants.Authorization.OrganisationRingFencing,
            Name = "Organisation Ring Fencing",
            Description = "Organisation Ring Fencing"
        },
        new()
        {
            Id = FeatureConstants.Authorization.ExternalApplicationIntegration,
            Name = "External Application Integration",
            Description = "External Application Integration"
        },
        new()
        {
            Id = FeatureConstants.Authorization.OrganizationManagement,
            Name = "Organization Management",
            Description = "Organization Management"
        },
        new()
        {
            Id = FeatureConstants.Authorization.CustomRoles,
            Name = "Custom Roles",
            Description = "Custom Roles"
        },
        new()
        {
            Id = FeatureConstants.Authorization.UserInvites,
            Name = "User Invites",
            Description = "User Invites"
        },
    ];

    private readonly IEventBus eventBusGateway;

    public AuthorizationFeatureRegistrationProducer(IEventBus eventBusGateway)
    {
        this.eventBusGateway = eventBusGateway;
    }

    public async Task RegisterFeatures()
    {
        await eventBusGateway.PublishAsync<SupportedFeaturesUpdated>(new
        {
            EventId = Guid.NewGuid(),
            SourceSystemReference = ApplicationName,
            Features = features,
            Action = SupportedFeaturesAction.AddOrUpdate,
            TenantId = UserData.SystemTenantId,
            ActionUserId = UserData.SystemUsername,
            ActionUserType = UserTypes.SystemUsers
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