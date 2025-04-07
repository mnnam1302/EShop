using EShop.Shared.Contracts.Services.Identity.Permissions;
using EShop.Shared.EventBus.Services;
using EShop.Shared.Scoping.ResourceAccessControl;
using MassTransit;

namespace EShop.Identity.Infrastructure.Producers;

public class UserPermissionRegistration : IPermissionRegistrationService
{
    private const string ApplicationName = "Identity";
    private const string ModuleName = "Reports";
    private readonly IEventBusGateway _eventBusGateway;

    private static readonly ReportPermission[] Permissions = 
    [
        new ReportPermission
        {
            Id = PermissionConstants.ManageReportsPermissionId,
            Name = "Manage Reports",
            Description = "Allow user to manage reports.",
            RelatedTo = ModuleName
        }
    ];

    public UserPermissionRegistration(IEventBusGateway eventBusGateway)
    {
        _eventBusGateway = eventBusGateway;
    }

    public async Task RegisterPermissions()
    {
        await _eventBusGateway.PublishAsync<SupportedPermissionsUpdated>(new
        {
            SourceSystemReference = ApplicationName,
            Permissions = Permissions,
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