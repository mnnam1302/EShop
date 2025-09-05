namespace EShop.Shared.Scoping.ResourceAccessControl;

public static class FeatureConstants
{
    public const string InitialState = nameof(FeatureState.Enabled);

    public static class TenancyFeatures
    {
        public const string SystemFormatConfiguration_FeatureId = "Tenancy_SystemFormatConfiguration";
    }

    // Identity
    public const string Identity_ExternalApplicationIntegration_FeatureId = "Identity_ExternalApplicationIntegration";
    public const string Identity_UserInvites_FeatureId = "Identity_UserInvites";
    public const string Identity_OrganisationRingFencing_FeatureId = "Identity_OrganisationRingFencing";
    public const string Identity_CustomRoles_FeatureId = "Identity_CustomRoles";
    public const string Identity_EnableTenantSpecificSequences_FeatureId = "Identity_EnableTenantSpecificSequences";
}
    public static class IdentityFeatures
    {
        public const string ExternalApplicationIntegration_FeatureId = "Identity_ExternalApplicationIntegration";
        public const string UserInvites_FeatureId = "Identity_UserInvites";
        public const string OrganisationRingFencing_FeatureId = "Identity_OrganisationRingFencing";
        public const string CustomRoles_FeatureId = "Identity_CustomRoles";
    }

    public static class ConfigurationFeatures
    {
        public const string ProductBuilder_FeatureId = "Configration_ProductBuilder";
    }
}

public enum FeatureModules
{
    EShop_Identity,
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