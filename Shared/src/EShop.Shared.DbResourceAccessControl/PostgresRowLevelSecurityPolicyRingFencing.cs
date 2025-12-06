using EShop.Shared.DbResourceAccessControl.Options;
using EShop.Shared.DomainTools.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EShop.Shared.DbResourceAccessControl;

internal sealed class PostgresRowLevelSecurityPolicyRingFencing : PostgresRowLevelSecurityPolicyBaseStrategy, IRingFencingIsolationStrategy
{
    private const string PolicyName = "tenant_isolation_with_ring_fencing";

    private readonly ILogger<PostgresRowLevelSecurityPolicyRingFencing> _logger;
    private readonly IOptions<DbResourceAccessControlOptions> _options;

    public PostgresRowLevelSecurityPolicyRingFencing(
        ILogger<PostgresRowLevelSecurityPolicyRingFencing> logger,
        IOptions<DbResourceAccessControlOptions> options)
        : base(PolicyName, logger)
    {
        _logger = logger;
        _options = options;
        this.AdjustRlsPolicyOnStartUp = options.Value.AdjustRingFencingRlsPoliciesOnStartUp;
    }

    protected override bool AdjustRlsPolicyOnStartUp { get; }

    public void AddRingFencingIsolation(DbContext dbContext)
    {
        var tableNames = GetTableNamesForEntitiesFoundIn(dbContext, entityType =>
                typeof(IRingFenced).IsAssignableFrom(entityType.ClrType) &&
                !typeof(IAllowWildcardRingFencing).IsAssignableFrom(entityType.ClrType));

        var rlsUsingExpression = _options.Value.UseNewRingFencingRlsExpression ? GetNewRlsExpression() : GetOldRlsExpression();

        AddIsolationStrategy(dbContext, tableNames, rlsUsingExpression);
    }

    private static string GetNewRlsExpression()
    {
        /*
         * The following RLS expression will create filter expressions that look like
           SELECT *
           FROM public."QuoteReadModels"
           WHERE "TenantId" = CURRENT_SETTING('app.tenant_id') -- filter by tenant
               AND
               (
                   "Scope" LIKE CURRENT_SETTING('app.scope') -- filter by scope as before
                   OR
                   (
                       CURRENT_SETTING('app.scope') LIKE '%|*|_%' -- app.scope is a wildcard scope, e.g. root-org|*|customer-id%
                       AND
                       RIGHT(CURRENT_SETTING('app.scope'), 1) = '%' -- app.scope ends with %
                       AND
                       -- "Scope" starts with the part before *, e.g. root-org|
                       STARTS_WITH("Scope", LEFT(CURRENT_SETTING('app.scope'), POSITION('|*' IN CURRENT_SETTING('app.scope'))))
                       AND
                       -- "Scope" ends with the part after * without the trailing %, e.g. |customer-id
                       "Scope" LIKE CONCAT('%', LEFT(RIGHT(CURRENT_SETTING('app.scope'), -POSITION('*|' IN CURRENT_SETTING('app.scope'))), -1))
                   )
               )
         */
        const string rlsUsingExpression =
            "\"TenantId\" = CURRENT_SETTING('app.tenant_id')" // Match tenant ID
            + " AND (\"Scope\" LIKE CURRENT_SETTING('app.scope')" // (AND match scope as before
            + " OR (CURRENT_SETTING('app.scope') LIKE '%|*|_%'" // (OR app.scope is a wildcard scope, e.g. root-org|*|customer-id%
            + " AND RIGHT(CURRENT_SETTING('app.scope'), 1) = '%'" // AND app.scope ends with %
                                                                  // AND "Scope" starts with the part before *, e.g. root-org|
            + " AND STARTS_WITH(\"Scope\", LEFT(CURRENT_SETTING('app.scope'), POSITION('|*' IN CURRENT_SETTING('app.scope'))))"
            // AND "Scope" ends with the part after * without the trailing %, e.g. |customer-id))
            + " AND \"Scope\" LIKE CONCAT('%', LEFT(RIGHT(CURRENT_SETTING('app.scope'), -POSITION('*|' IN CURRENT_SETTING('app.scope'))), -1))))";

        return rlsUsingExpression;
    }

    private static string GetOldRlsExpression()
    {
        const string rlsUsingExpression = "\"TenantId\" = current_setting('app.tenant_id')::VARCHAR(50) AND \"Scope\" LIKE current_setting('app.scope')::VARCHAR(500)";
        return rlsUsingExpression;
    }
}
