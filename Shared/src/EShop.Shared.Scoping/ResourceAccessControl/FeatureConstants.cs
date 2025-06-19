namespace EShop.Shared.Scoping.ResourceAccessControl;

public static class FeatureConstants
{
    public const string InitialState = nameof(FeatureState.Enabled);

    // Tenancy
    public const string Tenancy_SystemFormatConfiguration_FeatureId = "Tenancy_SystemFormatConfiguration";

    // Identity
    public const string Identity_ExternalApplicationIntegration_FeatureId = "Identity_ExternalApplicationIntegration";
    public const string Identity_UserInvites_FeatureId = "Identity_UserInvites";
    public const string Identity_OrganisationRingFencing_FeatureId = "Identity_OrganisationRingFencing";
    public const string Identity_CustomRoles_FeatureId = "Identity_CustomRoles";
    public const string Identity_EnableTenantSpecificSequences_FeatureId = "Identity_EnableTenantSpecificSequences";
}


public enum FeatureModules
{
    EShop_Identity,
    EShop_Tenancy,
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