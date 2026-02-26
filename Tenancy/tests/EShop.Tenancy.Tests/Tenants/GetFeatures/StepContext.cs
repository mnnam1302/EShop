using EShop.Tenancy.Domain.Entities;
using EShop.Tenancy.Persistence;
using EShop.Tenancy.Tests.Setups;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Tenancy.Tests.Tenants.GetFeatures;

internal sealed class StepContext(ApiContext apiContext)
{
    internal async Task<List<TenantFeature>> GetTenantFeaturesAsync(string tenantId)
    {
        var tenancyDbContext = apiContext.ServiceProvider.GetRequiredService<TenancyDbContext>();

        var tenantFeatures = await tenancyDbContext.TenantFeatures
            .Where(tf => tf.TenantId == tenantId)
            .ToListAsync();

        return tenantFeatures;
    }
}