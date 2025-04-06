namespace EShop.Shared.Contracts.Services.Identity.Permissions;

public interface SupportedPermissionsUpdated : IdentityEvent
{
    public string SourceSystemReference { get; }

    public Permission[] Permissions { get; }

    public SupportedPermissionAction Action { get; }
}

public interface Permission
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public string RelatedTo { get; }
}

public enum SupportedPermissionAction
{
    Added,
    Removed
}