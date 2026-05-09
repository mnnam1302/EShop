using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.DbResourceAccessControl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Inventory.Infrastructure;

public sealed class DbInitializer
{
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly ILogger<DbInitializer> _logger;
    private readonly InventoryDbContext dbContext;
    private readonly ITenantIsolationStrategy tenantIsolationStrategy;

    public DbInitializer(
        IUserDetailsProvider userDetailsProvider,
        ILogger<DbInitializer> logger,
        InventoryDbContext dbContext,
        ITenantIsolationStrategy tenantIsolationStrategy)
    {
        _userDetailsProvider = userDetailsProvider;
        _logger = logger;
        this.dbContext = dbContext;
        this.tenantIsolationStrategy = tenantIsolationStrategy;
    }

    public async Task Initialize(bool applyMigrations = true, bool applyTenantIsolation = true)
    {
        try
        {
            _userDetailsProvider.SetSystemUserContextWithEmptyScope();

            if (applyMigrations)
            {
                _logger.LogDebug("Applying any pending migrations...");
                await dbContext.Database.MigrateAsync();
            }
            else
            {
                _logger.LogInformation("Ensuring database is created without running migrations...");
                await dbContext.Database.EnsureCreatedAsync();
            }

            //ringFencingIsolationStrategy.AddRingFencingIsolation(dbContext);
            tenantIsolationStrategy.AddTenantIsolation(dbContext, appliedRingFencing: true);

        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Database initialization error");
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }
}