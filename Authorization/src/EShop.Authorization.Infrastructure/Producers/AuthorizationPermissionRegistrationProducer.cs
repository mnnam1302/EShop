using EShop.Shared.Contracts.IntegrationEvents.Authorization;
using EShop.Shared.EventBus.Abstractions;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Authorization.Infrastructure.Producers;

internal sealed class AuthorizationPermissionRegistrationProducer : IPermissionRegistrationService
{
    private const string ModuleName = "Authorization";

    private static readonly ReportPermission[] Permissions = [];

    private readonly IEventBusGateway eventBusGateway;

    public AuthorizationPermissionRegistrationProducer(IEventBusGateway eventBusGateway)
    {
        this.eventBusGateway = eventBusGateway;
    }

    public async Task RegisterPermissions()
    {
        await eventBusGateway.PublishAsync<SupportedPermissionsUpdated>(new
        {
            EventId = Guid.NewGuid(),
            SourceSystemReference = ModuleName,
            Permissions,
            Action = SupportedPermissionAction.Added,
            TenantId = string.Empty,
            ActionUserId = string.Empty,
            ActionUserType = string.Empty
        });
    }

    private sealed class ReportPermission : IPermission
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string RelatedTo { get; init; }
    }
}
