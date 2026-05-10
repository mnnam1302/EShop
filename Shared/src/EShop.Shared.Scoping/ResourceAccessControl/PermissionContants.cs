namespace EShop.Shared.Scoping.ResourceAccessControl;

public static class PermissionConstants
{
    public static class Tenancy
    {
        public const string ViewSystemSettings = "Tenancy_ViewSystemSettings";
        public const string ManageSystemSettings = "Tenancy_ManageSystemSettings";
    }

    public static class Authorization
    {
        public const string ViewOrganizations = "Authorization_ViewOrganizations";
        public const string ManageOrganizations = "Authorization_ManageOrganizations";

        public const string ViewUsers = "Authorization_ViewUsers";
        public const string ManageUsers = "Authorization_ManageUsers";

        public const string ViewRoles = "Authorization_ViewRoles";
        public const string ManageRoles = "Authorization_ManageRoles";

        public const string ViewCustomerUsers = "Authorization_ViewCustomerUsers";
        public const string ManageCustomerUsers = "Authorization_ManageCustomerUsers";

        public const string ViewPortalUserAccounts = "Authorization_ViewPortalUserAccounts";
        public const string ManagePortalUserAccounts = "Authorization_ManagePortalUserAccounts";
    }

    public static class ConfigurationPermissions
    {
        public const string ViewProductsPermissionId = "Configuration_ViewProducts";
        public const string ManageProductsPermissionId = "Configuration_ManageProducts";
    }

    public static class Catalog
    {
        public const string ManageCategories = "Catalog_ManageCategories";
        public const string ViewCategories = "Catalog_ViewCategories";

        public const string ViewProducts = "Catalog_ViewProducts";
        public const string ManageProducts = "Catalog_ManageProducts";
    }

    public static class Inventory
    {
        public const string ViewInventory = "Inventory_ViewInventory";
        public const string ManageInventory = "Inventory_ManageInventory";
    }
}
