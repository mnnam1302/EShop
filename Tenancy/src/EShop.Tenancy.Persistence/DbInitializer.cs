using EShop.Shared.DbResourceAccessControl;
using EShop.Shared.Scoping;
using EShop.Tenancy.Domain.Entities;
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
            _userDetailsProvider.SetSystemUserContextWithEmptyScope();

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

            await SeedTenantAsync(UserData.EShopSupportGroup, $"{UserData.EShopSupportGroup.ToLowerInvariant()}@eshop.ecommerce", "Root system organization");
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

    private async Task SeedTenantAsync(string name, string email, string description)
    {
        var tenant = new Tenant(name, name, name, email, null, description);

        if (await _tenancyDbContext.Tenants.AnyAsync(org => org.Id == name || org.Name == name))
        {
            _tenancyDbContext.Update(tenant);
        }
        else
        {
            _tenancyDbContext.Add(tenant);
        }
    }
}