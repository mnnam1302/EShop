using EShop.Catalog.Application.Shared;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.DbResourceAccessControl;
using EShop.Shared.Scoping;
using EShop.Shared.Sequences;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EShop.Catalog.Application.Boostrapping;

public sealed class DbInitializer(
    ILogger<DbInitializer> logger,
    IOptions<CatalogSequenceOptions> options,
    CatalogDbContext dbContext,
    ISequenceRegistry sequenceRegistry,
    IUserDetailsProvider userDetailsProvider,
    ITenantIsolationStrategy tenantIsolationStrategy)
{
    public async Task Initialize(bool applyMigrations = true, bool applyTenantIsolation = true)
    {
        try
        {
            userDetailsProvider.SetSystemUserContextWithEmptyScope();

            if (applyMigrations)
            {
                logger.LogDebug("Applying any pending migrations...");
                await dbContext.Database.MigrateAsync();
            }
            else
            {
                logger.LogInformation("Ensuring database is created without running migrations...");
                await dbContext.Database.EnsureCreatedAsync();
            }

            //ringFencingIsolationStrategy.AddRingFencingIsolation(dbContext);

            tenantIsolationStrategy.AddTenantIsolation(dbContext, appliedRingFencing: true);

            await sequenceRegistry.RegisterSequences(
                Program.ApplicationName,
                CatalogSequence.CategoryReference,
                options.Value.CategoryReferenceSeed,
                options.Value.TenantSequences);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialization error");
        }
        finally
        {
            userDetailsProvider.ClearSystemUserContext();
        }
    }
}
