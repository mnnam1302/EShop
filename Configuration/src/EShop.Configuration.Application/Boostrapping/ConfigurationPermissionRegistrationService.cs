using EShop.Shared.Contracts.Services.Identity.Permissions;
using EShop.Shared.EventBus.Services;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Configuration.Application.Boostrapping;

public class ConfigurationPermissionRegistrationService(IEventBusGateway eventBusGateway) : IPermissionRegistrationService
{
    private const string ModuleName = "Product builder";

    private static readonly ConfigurationPermission[] Permissions = new[]
    {
        new ConfigurationPermission()
        {
            Id = PermissionConstants.ViewProductsPermissionId,
            Name = "View products",
            Description = "Allows users viewing list products, all product versions, and product version details in read-only mode.",
            RelatedTo = ModuleName
        },
        new ConfigurationPermission()
        {
            Id = PermissionConstants.ManageProductsPermissionId,
            Name = "Create Product",
            Description = "Allows creating products in the configuration service.",
            RelatedTo = ModuleName
        }
    };

    public async Task RegisterPermissions()
    {
        await eventBusGateway.PublishAsync<SupportedPermissionsUpdated>(new
        {
            SourceSystemReference = Program.ApplicationName,
            Permissions = Permissions,
            Action = SupportedPermissionAction.Added,
            TenantId = string.Empty,
            ActionUserId = string.Empty,
            ActionUserType = string.Empty
        });
    }

    private sealed class ConfigurationPermission : Permission
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string RelatedTo { get; init; }
    }
}


