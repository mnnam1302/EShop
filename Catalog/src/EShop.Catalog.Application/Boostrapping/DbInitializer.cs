using EShop.Catalog.Application.Shared;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.DbResourceAccessControl;
using EShop.Shared.Scoping;
using EShop.Shared.Sequences;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EShop.Catalog.Application.Boostrapping;

public sealed class DbInitializer
{
    private readonly ILogger<DbInitializer> _logger;
    private readonly IOptions<CatalogSequenceOptions> _referenceOptions;
    private readonly CatalogDbContext _dbContext;
    private readonly ISequenceRegistry _sequenceRegistry;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly ITenantIsolationStrategy _tenantIsolationStrategy;
    private readonly IRingFencingIsolationStrategy _ringFencingIsolationStrategy;

    public DbInitializer(
        ILogger<DbInitializer> logger,
        IOptions<CatalogSequenceOptions> options,
        CatalogDbContext dbContext,
        ISequenceRegistry sequenceRegistry,
        IUserDetailsProvider userDetailsProvider,
        ITenantIsolationStrategy tenantIsolationStrategy,
        IRingFencingIsolationStrategy ringFencingIsolationStrategy)
    {
        _logger = logger;
        _referenceOptions = options;
        _dbContext = dbContext;
        _sequenceRegistry = sequenceRegistry;
        _userDetailsProvider = userDetailsProvider;
        _tenantIsolationStrategy = tenantIsolationStrategy;
        _ringFencingIsolationStrategy = ringFencingIsolationStrategy;
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

            _ringFencingIsolationStrategy.AddRingFencingIsolation(_dbContext);

            _tenantIsolationStrategy.AddTenantIsolation(_dbContext, appliedRingFencing: true);

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
