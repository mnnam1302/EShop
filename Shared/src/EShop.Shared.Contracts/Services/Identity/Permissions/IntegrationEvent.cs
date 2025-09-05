namespace EShop.Shared.Contracts.Services.Identity.Permissions;

public interface ISupportedPermissionsUpdated : IdentityEvent
{
    public string SourceSystemReference { get; }

    public IPermission[] Permissions { get; }

    public SupportedPermissionAction Action { get; }
}

public interface IPermission
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