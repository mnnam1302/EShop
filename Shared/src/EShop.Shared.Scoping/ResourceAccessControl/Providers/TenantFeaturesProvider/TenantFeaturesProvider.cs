using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;

public sealed class TenantFeaturesProvider : ITenantFeaturesProvider
{
    private static readonly TimeSpan _lockExpiration = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan _lockRetryDelay = TimeSpan.FromMilliseconds(200);

    private readonly ITenantFeaturesCachingService _tenantFeaturesCachingService;
    private readonly TenancyHttpClient _tenantFeatureHttpClient;
    private readonly IDistributedLock _distributedLock;

    public TenantFeaturesProvider(
        ITenantFeaturesCachingService tenantFeaturesCachingService,
        TenancyHttpClient tenantFeatureHttpClient,
        IDistributedLock distributedLock)
    {
        _tenantFeaturesCachingService = tenantFeaturesCachingService;
        _tenantFeatureHttpClient = tenantFeatureHttpClient;
        _distributedLock = distributedLock;
    }

    public async Task<string[]> GetFeatures(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentNullException(nameof(tenantId), "TenantId is required");
        }

        // Step 1: check cache without lock
        var cached = await _tenantFeaturesCachingService.GetTenantFeatures(tenantId);
        if (cached.Length != 0)
        {
            return cached;
        }

        // Step 2: cache miss — acquire per-tenant lock to rebuild cache
        using var lockHandle = await _distributedLock.TryAcquireAsync(
            $"features:{tenantId}",
            _lockExpiration);

        if (lockHandle is not null)
        {
            // Step 3: double-check cache after acquiring lock
            cached = await _tenantFeaturesCachingService.GetTenantFeatures(tenantId);
            if (cached.Length != 0)
            {
                return cached;
            }

            // Step 4: We are the designated rebuilder
            var features = await _tenantFeatureHttpClient.GetTenantFeaturesAsync(tenantId);
            if (features.Length > 0)
            {
                await _tenantFeaturesCachingService.AddTenantFeatures(tenantId, features);
            }

            return features;
        }

        // Step 5: Lock busy — another instance is rebuilding; wait and serve from cache
        await Task.Delay(_lockRetryDelay);
        return await _tenantFeaturesCachingService.GetTenantFeatures(tenantId);
    }
}
