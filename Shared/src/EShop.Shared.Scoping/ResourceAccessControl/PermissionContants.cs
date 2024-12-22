namespace EShop.Shared.Scoping.ResourceAccessControl;

public static class PermissionConstants
{
    // Service Users | Identity
    public const string ViewUsersPermissionId = "Users_ViewUsers";
    public const string ManageUsersPermissionId = "Users_ManageUsers";

    public const string ViewOrganizationsPermissionId = "Users_ViewOrganizations";
    public const string ManageOrganizationsPermissionId = "Users_ManageOrganizations";

    public const string ViewRolesPermissionId = "Users_ViewRoles";
    public const string ManageRolesPermissionId = "Users_ManageRoles";

    public const string ViewPortalUserAccountsPermissionId = "Users_ViewPortalUserAccounts";
    public const string ManagePortalUserAccountsPermissionId = "Users_ManagePortalUserAccounts";

    public const string ViewSystemSettingsPermissionId = "Users_ViewSystemSettings";
    public const string ManageSystemSettingsPermissionId = "Users_ManageSystemSettings";

    // Serice Account
    public const string UserViewAccountsPermissionId = "Accounts_ViewAccounts";

    public const string UserCreateAccountsPermissionId = "Accounts_CreateAccounts";
    public const string UserUpdateAccountsPermissionId = "Accounts_UpdateAccounts";
    public const string UserDeleteAccountsPermissionId = "Accounts_DeleteAccounts";
    public const string UserExportAccountsPermissionId = "Accounts_ExportAccounts";
    public const string UserImportAccountsPermissionId = "Accounts_ImportAccounts";

    // Catalogs
    public const string ViewCategoriesPermissionId = "Catalogs_ViewCategories";
    public const string ManageCategoriesPermissionId = "Catalogs_ManageCategories";

    public const string ViewProductsPermissionId = "Catalogs_ViewProducts";
    public const string ManageProductsPermissionId = "Catalogs_ManageProducts";

}