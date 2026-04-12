using EShop.Shared.Authentication;
using EShop.Shared.Contracts.Services.Authorization;
using EShop.Shared.EventBus.Abstractions;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Authorization.Infrastructure.Producers;

internal sealed class AuthorizationPermissionRegistrationProducer : IPermissionRegistrationService
{
    private const string ModuleName = "Authorization";

    private static readonly ReportPermission[] Permissions = [];

    private readonly IEventBus eventBusGateway;

    public AuthorizationPermissionRegistrationProducer(IEventBus eventBusGateway)
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
            TenantId = UserData.SystemTenantId,
            ActionUserId = UserData.SystemUsername,
            ActionUserType = UserTypes.SystemUsers
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
