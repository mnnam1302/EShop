using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;

namespace EShop.Testing.JsonApiApplication.Providers;

public class TestTenantFeatureProvider : ITenantFeaturesProvider
{
    private readonly Dictionary<string, List<string>> tenantFeatures = new();

    public void AddTenantFeature(string tenantId, string featureId)
    {
        if (!tenantFeatures.ContainsKey(tenantId))
        {
            tenantFeatures.Add(tenantId, new List<string>());
        }

        if (!tenantFeatures[tenantId].Contains(featureId))
        {
            tenantFeatures[tenantId].Add(featureId);
        }
    }

    public Task<string[]> GetFeatures(string tenantId)
    {
        var features = tenantFeatures.ContainsKey(tenantId) ? tenantFeatures[tenantId].ToArray() : Array.Empty<string>();
        return Task.FromResult(features);
    }

    public void RemoveAllFeatures(string tenantId)
    {
        if (tenantFeatures.ContainsKey(tenantId))
        {
            tenantFeatures[tenantId].Clear();
        }
    }

    public void RemoveFeatureFromTenant(string tenantId, string featureId)
    {
        if (tenantFeatures.ContainsKey(tenantId) && tenantFeatures[tenantId].Contains(featureId))
        {
            tenantFeatures[tenantId].Remove(featureId);
        }
    }
}