using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;

namespace EShop.Testing.JsonApiApplication.Providers;

public sealed class TestTenantFeaturesCachingService : ITenantFeaturesCachingService
{
    private readonly Dictionary<string, List<string>> tenantFeatures = new();

    public Task AddTenantFeatures(string tenantId, string[] features, CancellationToken cancellationToken = default)
    {
        if (!tenantFeatures.ContainsKey(tenantId))
        {
            tenantFeatures.Add(tenantId, new List<string>());
        }

        foreach (var feature in features)
        {
            if (!tenantFeatures[tenantId].Contains(feature))
            {
                tenantFeatures[tenantId].Add(feature);
            }
        }

        return Task.CompletedTask;
    }

    public Task<string[]> GetTenantFeatures(string tenantId, CancellationToken cancellationToken = default)
    {
        var features = tenantFeatures.TryGetValue(tenantId, out List<string>? value)
            ? value.ToArray()
            : Array.Empty<string>();

        return Task.FromResult(features);
    }

    public Task RemoveTenantFeatures(string tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantFeatures.ContainsKey(tenantId))
        {
            tenantFeatures[tenantId].Clear();
        }

        return Task.CompletedTask;
    }
}
