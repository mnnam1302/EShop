namespace EShop.Shared.Scoping.ResourceAccessControl;

public static class PermissionConstants
{
    // Documents
    public const string ManageTemplatesPermissionId = "Documents_ManageTemplates";

    public const string ViewTemplatesPermissionId = "Documents_ViewTemplates";
    public const string ManageDocumentsPermissionId = "Documents_ManageDocuments";
    public const string ViewDocumentsPermissionId = "Documents_ViewDocuments";
    public const string GenerateDocumentsPermissionId = "Documents_GenerateDocuments";
    public const string ExtractInfoFromDocumentsPermissionId = "Documents_ExtractInfoFromDocuments";

    // Users
    public const string ViewUsersPermissionId = "Users_ViewUsers";

    public const string ManageUsersPermissionId = "Users_ManageUsers";
    public const string ViewPortalUserAccountsPermissionId = "Users_ViewPortalUserAccounts";
    public const string ManagePortalUserAccountsPermissionId = "Users_ManagePortalUserAccounts";

    // System Settings
    public const string ViewSystemSettingsPermissionId = "Users_ViewSystemSettings";

    public const string ManageSystemSettingsPermissionId = "Users_ManageSystemSettings";

    // Roles
    public const string ViewRolesPermissionId = "Users_ViewRoles";
    public const string ManageRolesPermissionId = "Users_ManageRoles";

    // Account
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
}