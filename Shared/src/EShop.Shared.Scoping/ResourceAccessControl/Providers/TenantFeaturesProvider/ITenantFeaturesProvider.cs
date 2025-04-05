namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;

public interface ITenantFeaturesProvider
{
    Task<string[]> GetFeatures(string tenantId);
}