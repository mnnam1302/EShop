using EShop.Configuration.Application.Shared;
using EShop.Shared.DbResourceAccessControl;
using EShop.Shared.Scoping;
using Microsoft.EntityFrameworkCore;

namespace EShop.Configuration.Application.Boostrapping;

public class DbInitializer
{
    private readonly ConfigurationDbContext _dbContext;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly ITenantIsolationStrategy _tenantIsolationStrategy;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public DbInitializer(
        ConfigurationDbContext dbContext,
        IUserDetailsProvider userDetailsProvider,
        ITenantIsolationStrategy tenantIsolationStrategy,
        IConfiguration configuration,
        ILogger<DbInitializer> logger)
    {
        _dbContext = dbContext;
        _userDetailsProvider = userDetailsProvider;
        _tenantIsolationStrategy = tenantIsolationStrategy;
        _configuration = configuration;
        _logger = logger;
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

            if (applyTenantIsolation && _configuration.GetValue("AllowTenantIsolation", true))
            {
                _tenantIsolationStrategy.AddTenantIsolation(_dbContext);
            }

            await _dbContext.SaveChangesAsync();
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