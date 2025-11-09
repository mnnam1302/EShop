using EShop.Shared.Authentication;
using EShop.Shared.Contracts.Services.Identity.Permissions;
using EShop.Shared.EventBus.Services;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using static EShop.Shared.Scoping.ResourceAccessControl.PermissionConstants;

namespace EShop.Catalog.Application.Boostrapping;

public sealed class CatalogPermissionRegistrationService : IPermissionRegistrationService
{
    private readonly IEventBusGateway eventBus;

    public CatalogPermissionRegistrationService(IEventBusGateway eventBus)
    {
        this.eventBus = eventBus;
    }

    private const string ModuleName = "Product builder";

    private static readonly CatalogPermission[] Permissions = new[]
    {
        new CatalogPermission()
        {
            Id = PermissionConstants.Catalog.ViewProducts,
            Name = "View products",
            Description = "Allows users viewing list products, all product versions, and product version details in read-only mode.",
            RelatedTo = ModuleName
        },
        new CatalogPermission()
        {
            Id = PermissionConstants.Catalog.ManageProducts,
            Name = "Manage products",
            Description = "Allows users adding new product, adding/editing/cloning product versions, publishing product, activating published product version",
            RelatedTo = ModuleName
        }
    };

    public async Task RegisterPermissions()
    {
        await eventBus.PublishAsync<ISupportedPermissionsUpdated>(new
        {
            SourceSystemReference = Program.ApplicationName,
            Permissions = Permissions,
            Action = SupportedPermissionAction.Added,
            TenantId = string.Empty,
            ActionUserId = UserData.SystemUsername,
            ActionUserType = string.Empty
        });
    }

    private sealed class CatalogPermission : IPermission
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string RelatedTo { get; init; }
    }
}
