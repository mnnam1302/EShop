using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.Scoping.ResourceAccessControl;
using MassTransit;

namespace EShop.Identity.Infrastructure.Producers;

public class UserFeatureRegistrationService : IFeatureRegistrationService
{
    private readonly string ApplicationName = "Identity";
    private readonly IPublishEndpoint _publishEndpoint;

    public UserFeatureRegistrationService(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

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
        await _publishEndpoint.Publish<SupportedFeaturesUpdated>(new
        {
            SourceSystemReference = ApplicationName,
            Features = features,
            TenantId = string.Empty,
            Action = SupportedFeaturesAction.AddOrUpdate,
            ActionUserId = string.Empty
        });
    }

    private sealed class IdentityFeature : Feature
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public string Module => nameof(FeatureModules.EShop_Identity);
        public string State => nameof(FeatureState.Disabled);
    }
}