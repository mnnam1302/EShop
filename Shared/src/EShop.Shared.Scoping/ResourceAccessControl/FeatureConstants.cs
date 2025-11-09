namespace EShop.Shared.Scoping.ResourceAccessControl;

public static class FeatureConstants
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
        public const string ProductBuilder_FeatureId = "Catalog_ProductBuilder";
    }
}

public enum FeatureModules
{
    EShop_Identity,
    EShop_Authorization,
    EShop_Tenancy,
    EShop_Configuration,
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