using EShop.Shared.Authentication;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Services.Authorization;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Catalog.Application.Bootstrapping;

public sealed class CatalogPermissionRegistrationService(IEventBus eventBus) : IPermissionRegistrationService
{
    private const string ModuleName = "Product builder";

    private static readonly CatalogPermission[] permissions =
    [
        new CatalogPermission
        {
            Id = PermissionConstants.Catalog.ViewCategories,
            Name = "View categories",
            Description = "Allows users viewing list categories and category details in read-only mode.",
            RelatedTo = ModuleName
        },
        new CatalogPermission
        {
            Id = PermissionConstants.Catalog.ManageCategories,
            Name = "Manage categories",
            Description = "Allows users adding/editing/cloning/deleting categories.",
            RelatedTo = ModuleName
        },
        new CatalogPermission
        {
            Id = PermissionConstants.Catalog.ViewProducts,
            Name = "View products",
            Description = "Allows users viewing list products, all product versions, and product version details in read-only mode.",
            RelatedTo = ModuleName
        },
        new CatalogPermission
        {
            Id = PermissionConstants.Catalog.ManageProducts,
            Name = "Manage products",
            Description = "Allows users adding new product, adding/editing/cloning product versions, publishing product, activating published product version",
            RelatedTo = ModuleName
        }
    ];

    public async Task RegisterPermissions()
    {
        await eventBus.PublishAsync(new SupportedPermissionsUpdated
        {
            SourceSystemReference = Program.ApplicationName,
            Permissions = permissions,
            Action = SupportedPermissionAction.Added,
            TenantId = UserData.SystemTenantId,
            ActionUserId = UserData.SystemUsername,
            ActionUserType = UserTypes.SystemUsers
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