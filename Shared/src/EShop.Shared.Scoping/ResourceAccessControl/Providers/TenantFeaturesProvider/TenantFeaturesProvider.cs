namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;

public class TenantFeaturesProvider : ITenantFeaturesProvider
{
    private readonly ITenantFeaturesCachingService _tenantFeaturesCachingService;
    private readonly TenantFeaturesHttpClient _tenantFeatureHttpClient;

    public TenantFeaturesProvider(ITenantFeaturesCachingService tenantFeaturesCachingService, TenantFeaturesHttpClient tenantFeatureHttpClient)
    {
        _tenantFeaturesCachingService = tenantFeaturesCachingService;
        _tenantFeatureHttpClient = tenantFeatureHttpClient;
    }

    public async Task<string[]> GetFeatures(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentNullException(nameof(tenantId), "TenantId is required");
        }

        var cachedFeatureIds = await _tenantFeaturesCachingService.GetTenantFeatures(tenantId);
        if (cachedFeatureIds.Length != 0)
        {
            return cachedFeatureIds;
        }

        var featuresFromApi = await _tenantFeatureHttpClient.GetTenantFeaturesAsync(tenantId);

        return featuresFromApi;
    }
}