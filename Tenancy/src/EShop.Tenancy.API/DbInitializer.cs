using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.DbResourceAccessControl;
using EShop.Tenancy.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EShop.Tenancy.API;

public sealed class DbInitializer(
    TenancyDbContext tenancyDbContext,
    IUserDetailsProvider userDetailsProvider,
    ITenantIsolationStrategy tenantIsolationStrategy,
    IConfiguration configuration,
    ILogger<DbInitializer> logger)
{
    public async Task Initialize(bool applyMigrations = true, bool applyTenantIsolation = true)
    {
        using var scope = userDetailsProvider.CreateSystemUserScope(tenantId: null);

        try
        {
            if (applyMigrations)
            {
                logger.LogDebug("Applying any pending migrations...");
                await tenancyDbContext.Database.MigrateAsync();
            }
            else
            {
                logger.LogInformation("Ensuring database is created without running migrations...");
                await tenancyDbContext.Database.EnsureCreatedAsync();
            }

            if (applyTenantIsolation && configuration.GetValue<bool>("AllowTenantIsolation", true))
            {
                tenantIsolationStrategy.AddTenantIsolation(tenancyDbContext);
            }

            await tenancyDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialization error");
        }
    }
}