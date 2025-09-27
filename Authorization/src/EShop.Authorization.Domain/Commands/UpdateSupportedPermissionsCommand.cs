using EShop.Shared.Contracts.Services.Identity.Permissions;
using EShop.Shared.CQRS.Command;

namespace EShop.Authorization.Domain.Commands;

public sealed class UpdateSupportedPermissionsCommand : ICommand
{
    public required string SourceSystemReference { get; init; }
    public IPermission[] Permissions { get; init; } = [];
    public SupportedPermissionAction Action { get; init; }
}
