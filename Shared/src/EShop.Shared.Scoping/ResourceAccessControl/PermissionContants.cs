namespace EShop.Shared.Scoping.ResourceAccessControl;

public static class PermissionConstants
{
    // Identity
    public const string ViewSystemSettingsPermissionId = "Identity_ViewSystemSettings";
    public const string ManageSystemSettingsPermissionId = "Identity_ManageSystemSettings";

    public const string ViewOrganizationsPermissionId = "Identity_ViewOrganizations";
    public const string ManageOrganizationsPermissionId = "Identity_ManageOrganizations";

    public const string ViewRolesPermissionId = "Identity_ViewRoles";
    public const string ManageRolesPermissionId = "Identity_ManageRoles";

    public const string ViewUsersPermissionId = "Identity_ViewUsers";
    public const string ManageUsersPermissionId = "Identity_ManageUsers";

    public const string ViewCustomerUsersPermissionId = "Identity_ViewCustomerUsers";
    public const string ManageCustomerUsersPermissionId = "Identity_ManageCustomerUsers";

    public const string ViewPortalUserAccountsPermissionId = "Identity_ViewPortalUserAccounts";
    public const string ManagePortalUserAccountsPermissionId = "Identity_ManagePortalUserAccounts";

    // Reports
    public const string ManageReportsPermissionId = "Reports_ManageReports";

    // Accounts
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