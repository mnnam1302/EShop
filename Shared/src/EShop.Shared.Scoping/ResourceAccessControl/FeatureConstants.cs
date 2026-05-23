namespace EShop.Shared.Scoping.ResourceAccessControl;

public class FeatureConstants
{
    public const string InitialState = nameof(FeatureState.Enabled);

    public static class Tenancy
    {
        public const string SystemFormatConfiguration_FeatureId = "Tenancy_SystemFormatConfiguration";
    }

    public static class Authorization
    {
        public const string OrganisationRingFencing = "Authorization_OrganisationRingFencing";
        public const string EnableTenantSpecificSequences = "Authorization_EnableTenantSpecificSequences";

        public const string OrganizationManagement = "Authorization_OrganizationManagement";
        public const string ExternalApplicationIntegration = "Authorization_ExternalApplicationIntegration";
        public const string UserInvites = "Authorization_UserInvites";
        public const string CustomRoles = "Authorization_CustomRoles";
    }

    public static class Catalog
    {
        public const string ProductFeatureId = "Catalog_ProductBuilder";
    }

    public static class Inventory
    {
        public const string InventoryManagement = "Inventory_InventoryManagement";
    }
}

public enum FeatureModules
{
    EShop_Authorization,
    EShop_Tenancy,
    EShop_Catalog,
    EShop_Inventory
}

public enum FeatureState
{
    NotAvailable,
    Enabled,
    Disabled
}

public enum FeatureCategory
{
    Permanent,
    Release,
    Experiment,
    Operational
}
