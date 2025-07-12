using EShop.Shared.Contracts.Abstractions.Requests;

namespace EShop.Shared.Contracts.Services.Identity.Permissions;

public static class Command
{
    public record UpdateSupportedPermissionsCommandInternal : ICommand
    {
        public required string SourceSystemReference { get; init; }

        public IPermission[] Permissions { get; init; }

        public SupportedPermissionAction Action { get; init; }
    }
}