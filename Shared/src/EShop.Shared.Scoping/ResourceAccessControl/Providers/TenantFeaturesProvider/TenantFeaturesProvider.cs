namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;

public class TenantFeaturesProvider : IUserFeaturesProvider
{
    private readonly ITenantFeaturesCachingService _tenantFeaturesCachingService;
    private readonly TenantFeaturesHttpClient _tenantFeatureHttpClient;

    public TenantFeaturesProvider(ITenantFeaturesCachingService tenantFeaturesCachingService, TenantFeaturesHttpClient tenantFeatureHttpClient)
    {
        _tenantFeaturesCachingService = tenantFeaturesCachingService;
        _tenantFeatureHttpClient = tenantFeatureHttpClient;
    }

    public async Task<string[]> GetFeatures(string userId, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentNullException("TenantId is required", nameof(tenantId));
        }

        var cachedFeatureIds = await _tenantFeaturesCachingService.GetTenantFeatures(tenantId);
        if (cachedFeatureIds.Any())
        {
            return cachedFeatureIds;
        }

        var featuresFromApi = await _tenantFeatureHttpClient.GetTenantFeaturesAsync(tenantId);

        return featuresFromApi;
    }
}