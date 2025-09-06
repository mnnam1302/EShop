using EShop.Catalog.Application.Shared;
using EShop.Shared.DbResourceAccessControl;
using EShop.Shared.Scoping;
using EShop.Shared.Sequences;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EShop.Catalog.Application.Boostrapping;

public sealed class DbInitializer
{
    private readonly CatalogDbContext _dbContext;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly ITenantIsolationStrategy _tenantIsolationStrategy;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly ISequenceRegistry _sequenceRegistry;
    private readonly IOptions<CatalogSequenceOptions> _referenceOptions;

    public DbInitializer(
        CatalogDbContext dbContext,
        IUserDetailsProvider userDetailsProvider,
        ITenantIsolationStrategy tenantIsolationStrategy,
        IConfiguration configuration,
        ILogger<DbInitializer> logger,
        ISequenceRegistry sequenceRegistry,
        IOptions<CatalogSequenceOptions> options)
    {
        _dbContext = dbContext;
        _userDetailsProvider = userDetailsProvider;
        _tenantIsolationStrategy = tenantIsolationStrategy;
        _configuration = configuration;
        _logger = logger;
        _sequenceRegistry = sequenceRegistry;
        _referenceOptions = options;
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

            await _sequenceRegistry.RegisterSequences(
                Program.ApplicationName,
                CatalogSequence.CategoryReference,
                _referenceOptions.Value.CategoryReferenceSeed,
                _referenceOptions.Value.TenantSequences);
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
