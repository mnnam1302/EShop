namespace EShop.Shared.Scoping.ResourceAccessControl;

public static class PermissionConstants
{
    // Service Users | Identity
    public const string ViewUsersPermissionId = "Users_ViewUsers";
    public const string ManageUsersPermissionId = "Users_ManageUsers";

    // Roles
    public const string ViewRolesPermissionId = "Users_ViewRoles";
    public const string ManageRolesPermissionId = "Users_ManageRoles";

    // Serice Account
    public const string UserViewAccountsPermissionId = "Accounts_ViewAccounts";

    public const string UserCreateAccountsPermissionId = "Accounts_CreateAccounts";
    public const string UserUpdateAccountsPermissionId = "Accounts_UpdateAccounts";
    public const string UserExportAccountsPermissionId = "Accounts_ExportAccounts";
    public const string UserImportAccountsPermissionId = "Accounts_ImportAccounts";
    public const string UserViewHistoryPermissionId = "Accounts_ViewHistory";
    public const string UserViewContactsPermissionId = "Accounts_ViewContacts";
    public const string UserCreateContactsPermissionId = "Accounts_CreateContacts";
    public const string UserUpdateContactsPermissionId = "Accounts_UpdateContacts";
    public const string UserDeleteContactsPermissionId = "Accounts_DeleteContacts";
    public const string UpdateCustomerDiscountPermissionId = "Accounts_UpdateDiscount";
    public const string PerformCreditCheckPermissionId = "Accounts_PerformCreditCheck";

    // Catalogs
    public const string ViewCategoriesPermissionId = "Catalogs_ViewCategories";
    public const string ManageCategoriesPermissionId = "Catalogs_ManageCategories";

    public const string ViewProductsPermissionId = "Catalogs_ViewProducts";
    public const string ManageProductsPermissionId = "Catalogs_ManageProducts";

}