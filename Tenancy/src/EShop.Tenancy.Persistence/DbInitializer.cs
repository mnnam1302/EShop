using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.DbResourceAccessControl;
using EShop.Shared.Scoping;
using EShop.Tenancy.Domain.Entities;
using EShop.Testing.IntegrationTest;
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

            await SeedTenant();
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

    private const string TenantName = TestTenantSeedingScript.TenantName;
    private const string OwnerUserName = TestTenantSeedingScript.UserName;
    private const string EmailTenant = TestTenantSeedingScript.TenantEmail;
    private const string TenantDescription = TestTenantSeedingScript.TenantDescription;

    private async Task SeedTenant()
    {
        var command = new Command.CreateTenantCommand
        {
            Id = TenantName,
            Name = TenantName,
            OwnerUsername = OwnerUserName,
            Email = EmailTenant,
            PhoneNumber = "+477" + new Random().Next(0, 1000000000).ToString(),
            Description = TenantDescription
        };

        var tenant = Tenant.Create(command);

        if (await _tenancyDbContext.Tenants.AnyAsync(t => t.Id == TenantName || t.Name == TenantName))
        {
            _tenancyDbContext.Update(tenant);
        }
        else
        {
            _tenancyDbContext.Add(tenant);
        }
    }
}