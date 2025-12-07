namespace EShop.Shared.Contracts.Services.Authorization;

public sealed class SupportedPermissionsUpdated : AuthorizationIntegrationEvent
{
    public required string SourceSystemReference { get; init; }

    public IPermission[] Permissions { get; init; } = [];

    public SupportedPermissionAction Action { get; init; }
}

public interface IPermission
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    string RelatedTo { get; }
}

public enum SupportedPermissionAction
{
    Added,
    Removed
}
