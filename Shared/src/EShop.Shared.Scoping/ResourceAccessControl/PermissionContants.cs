namespace EShop.Shared.Scoping.ResourceAccessControl;

public static class PermissionConstants
{
    // Tenancy
    public const string ViewSystemSettingsPermissionId = "Tenancy_ViewSystemSettings";
    public const string ManageSystemSettingsPermissionId = "Tenancy_ManageSystemSettings";

    // Identity
    public const string ViewOrganizationsPermissionId = "Identity_ViewOrganizations";
    public const string ManageOrganizationsPermissionId = "Identity_ManageOrganizations";

    public const string ViewUsersPermissionId = "Identity_ViewUsers";
    public const string ManageUsersPermissionId = "Identity_ManageUsers";

    public const string ViewRolesPermissionId = "Identity_ViewRoles";
    public const string ManageRolesPermissionId = "Identity_ManageRoles";

    public const string ViewCustomerUsersPermissionId = "Identity_ViewCustomerUsers";
    public const string ManageCustomerUsersPermissionId = "Identity_ManageCustomerUsers";

    public const string ViewPortalUserAccountsPermissionId = "Identity_ViewPortalUserAccounts";
    public const string ManagePortalUserAccountsPermissionId = "Identity_ManagePortalUserAccounts";
}