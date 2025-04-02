namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;

public interface IUserFeaturesProvider
{
    Task<string[]> GetFeatures(string userId, string tenantId);
}