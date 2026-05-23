using EShop.Shared.Authentication;
using EShop.Shared.Contracts.Services.Authorization;
using EShop.Shared.EventBus;
using EShop.Shared.EventBus.Abstractions;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Inventory.Infrastructure.Producers;

internal sealed class InventoryPermissionRegistrationService : IPermissionRegistrationService
{
    private readonly IEventBus _eventBus;

    public InventoryPermissionRegistrationService(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    private const string _moduleName = "Inventory management";

    private static readonly InventoryPermission[] _permissions =
    [
        new()
        {
            Id = PermissionConstants.Inventory.ViewInventory,
            Name = "View inventory",
            Description = "Allows users to view inventory details in read-only mode.",
            RelatedTo = _moduleName
        },
        new()
        {
            Id = PermissionConstants.Inventory.ManageInventory,
            Name = "Manage inventory",
            Description = "Allows users to manage inventory.",
            RelatedTo = _moduleName
        }
    ];


    public async Task RegisterPermissions()
    {
        await _eventBus.PublishAsync(new SupportedPermissionsUpdated
        {
            SourceSystemReference = "Inventory",
            Permissions = _permissions,
            Action = SupportedPermissionAction.Added,
            TenantId = UserData.SystemTenantId,
            ActionUserId = UserData.SystemUsername,
            ActionUserType = UserTypes.SystemUsers
        });
    }

    private sealed class InventoryPermission : IPermission
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string RelatedTo { get; init; }
    }
}
