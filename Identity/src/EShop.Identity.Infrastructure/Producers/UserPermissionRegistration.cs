using EShop.Shared.Contracts.Services.Identity.Permissions;
using EShop.Shared.Scoping.ResourceAccessControl;
using MassTransit;

namespace EShop.Identity.Infrastructure.Producers;

public class UserPermissionRegistration : IPermissionRegistrationService
{
    private const string ApplicationName = "Identity";
    private const string ModuleName = "Reports";
    private readonly IPublishEndpoint _publishEndpoint;

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

    public UserPermissionRegistration(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task RegisterPermissions()
    {
        await _publishEndpoint.Publish<SupportedPermissionsUpdated>(new
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