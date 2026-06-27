using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.DbResourceAccessControl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Finance.Infrastructure;

public sealed class DbInitializer
{
    private readonly ILogger<DbInitializer> _logger;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly FinanceDbContext _dbContext;
    private readonly ITenantIsolationStrategy _tenantIsolationStrategy;

    public DbInitializer(
        ILogger<DbInitializer> logger,
        IUserDetailsProvider userDetailsProvider,
        FinanceDbContext dbContext,
        ITenantIsolationStrategy tenantIsolationStrategy)
    {
        _logger = logger;
        _userDetailsProvider = userDetailsProvider;
        _dbContext = dbContext;
        _tenantIsolationStrategy = tenantIsolationStrategy;
    }

    public async Task Initialize(bool applyMigrations = true, bool applyTenantIsolation = true)
    {
        try
        {
            _userDetailsProvider.SetSystemUserContextWithEmptyScope();

            if (applyMigrations)
            {
                _logger.LogDebug("Applying any pending migrations...");
                await _dbContext.Database.MigrateAsync();
            }
            else
            {
                _logger.LogInformation("Ensuring database is created without running migrations...");
                await _dbContext.Database.EnsureCreatedAsync();
            }

            _tenantIsolationStrategy.AddTenantIsolation(_dbContext, appliedRingFencing: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization error");
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }
}
