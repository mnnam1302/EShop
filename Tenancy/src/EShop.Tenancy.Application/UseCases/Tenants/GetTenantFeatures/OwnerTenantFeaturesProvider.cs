using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;
using EShop.Tenancy.Application.UseCases.Features;

namespace EShop.Tenancy.Application.UseCases.Tenants.GetTenantFeatures;

public class OwnerTenantFeaturesProvider : ITenantFeaturesProvider
{
    private readonly ITenantFeaturesCachingService _tenantFeaturesCachingService;
    private readonly IFeatureService _featureService;

    public OwnerTenantFeaturesProvider(ITenantFeaturesCachingService tenantFeaturesCachingService, IFeatureService featureService)
    {
        _tenantFeaturesCachingService = tenantFeaturesCachingService;
        _featureService = featureService;
    }

    public async Task<string[]> GetFeatures(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentNullException(nameof(tenantId), "TenantId is required");
        }

        var tenantFeaturesCache = await _tenantFeaturesCachingService.GetTenantFeatures(tenantId);
        if (tenantFeaturesCache.Length != 0)
        {
            return tenantFeaturesCache;
        }

        var calculatedFeatures = await _featureService.GetTenantFeaturesByTenantIdAsync(tenantId, nameof(FeatureState.Enabled));
        var featureIds = calculatedFeatures.Select(x => x.FeatureId).ToArray();

        await _tenantFeaturesCachingService.AddTenantFeatures(tenantId, featureIds);

        return featureIds;
    }
}
