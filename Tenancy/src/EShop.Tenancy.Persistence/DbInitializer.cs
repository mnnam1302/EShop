using EShop.Shared.DbResourceAccessControl;
using EShop.Shared.Scoping;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EShop.Tenancy.Persistence;

public class DbInitializer
{
    private readonly TenancyDbContext _tenancyDbContext;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly ITenantIsolationStrategy _tenantIsolationStrategy;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    private const string TenantName = "eshop-staging";
    private const string RoleName = "Owner";
    private const string UserName = "owner.staging@gmail.com";
    private const string DisplayName = "Owner Staging";

    public DbInitializer(
        TenancyDbContext tenancyDbContext,
        IUserDetailsProvider userDetailsProvider,
        ITenantIsolationStrategy tenantIsolationStrategy,
        IConfiguration configuration,
        ILogger<DbInitializer> logger)
    {
        _tenancyDbContext = tenancyDbContext;
        _userDetailsProvider = userDetailsProvider;
        _tenantIsolationStrategy = tenantIsolationStrategy;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Initialize(bool applyMigrations = true, bool applyTenantIsolation = true)
    {
        try
        {
            _userDetailsProvider.SetSystemUserContext(TenantName);

            if (applyMigrations)
            {
                _logger.LogDebug("Applying any pending migrations...");
                await _tenancyDbContext.Database.MigrateAsync();
            }
            else
            {
                _logger.LogInformation("Ensuring database is created without running migrations...");
                await _tenancyDbContext.Database.EnsureCreatedAsync();
            }

            if (applyTenantIsolation && _configuration.GetValue<bool>("AllowTenantIsolation", true))
            {
                _tenantIsolationStrategy.AddTenantIsolation(_tenancyDbContext);
            }

            await SeedFeaturesSystem();
            await _tenancyDbContext.SaveChangesAsync();
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

    private async Task SeedFeaturesSystem()
    {
        
    }
}