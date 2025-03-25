namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;

public interface ITenantFeaturesCachingService
{
    Task<string[]> GetTenantFeatures(string tenantId, CancellationToken cancellationToken = default);

    Task AddTenantFeatures(string tenantId, string[] features, CancellationToken cancellationToken = default);

    Task RemoveTenantFeatures(string tenantId, CancellationToken cancellationToken = default);
}