using EShop.Shared.Contracts.Services.Identity.Permissions;
using EShop.Shared.EventBus.Services;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Authorization.Application.Producers;

public sealed class AuthorizationPermissionRegistrationProducer : IPermissionRegistrationService
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
        await eventBusGateway.PublishAsync<ISupportedPermissionsUpdated>(new
        {
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