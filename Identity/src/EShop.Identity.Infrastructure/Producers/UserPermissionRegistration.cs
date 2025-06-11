using EShop.Shared.Contracts.Services.Identity.Permissions;
using EShop.Shared.EventBus.Services;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Identity.Infrastructure.Producers;

public class UserPermissionRegistration(IEventBusGateway eventBusGateway) : IPermissionRegistrationService
{
    private const string ApplicationName = "Identity";

    private static readonly ReportPermission[] Permissions = [];

    public async Task RegisterPermissions()
    {
        await eventBusGateway.PublishAsync<SupportedPermissionsUpdated>(new
        {
            SourceSystemReference = ApplicationName,
            Permissions,
            Action = SupportedPermissionAction.Added,
            TenantId = string.Empty,
            ActionUserId = string.Empty,
            ActionUserType = string.Empty
        });
    }

    private sealed class ReportPermission : Permission
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string RelatedTo { get; init; }
    }
}