namespace EShop.Shared.Contracts.IntegrationEvents.Authorization;

public interface SupportedPermissionsUpdated : AuthorizationEvent
{
    string SourceSystemReference { get; }

    IPermission[] Permissions { get; }

    SupportedPermissionAction Action { get; }
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
