using EShop.Shared.DomainTools.DependencyInjections;
using EShop.Shared.Scoping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace EShop.Shared.DbResourceAccessControl;

public sealed class PostgresRowLevelSecurityPolicyIsolation : PostgresRowLevelSecurityPolicyBaseStrategy, ITenantIsolationStrategy
{
    internal const string PolicyName = "tenant_isolation";

    public PostgresRowLevelSecurityPolicyIsolation(ILogger<PostgresRowLevelSecurityPolicyIsolation> logger)
        : base(PolicyName, logger)
    {
    }

    protected override bool AdjustRlsPolicyOnStartUp => false;

    public void AddTenantIsolation(DbContext dbContext)
    {
        IEnumerable<PropertyInfo> resourceProperties = dbContext.GetType().GetProperties()
            .Where(p => p.PropertyType.IsGenericType
            && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

        var undeclaredProperties = resourceProperties
            .Where(p => !typeof(IScoped).IsAssignableFrom(p.PropertyType.GetGenericArguments()[0])
                        && !typeof(IExcludedFromScoping).IsAssignableFrom(p.PropertyType.GetGenericArguments()[0]));

        if (undeclaredProperties.Any())
        {
            var invalidResources = undeclaredProperties.Select(p => p.Name).ToCommaSeparatedString();
            throw new InvalidOperationException($"Please make sure to declare scoping behaviour on following properties of '{dbContext.GetType().Name}': {invalidResources}.");
        }

        resourceProperties = resourceProperties
            .Where(x => typeof(IScoped).IsAssignableFrom(x.PropertyType.GetGenericArguments()[0]));

        // Ring fencing is applied later

        var tableNames = resourceProperties
            .Select(p => p.Name)
            .ToArray();

        const string rlsUsingExpression = "\"TenantId\" = current_setting('app.tenant_id')::VARCHAR(50)";
        AddIsolationStrategy(dbContext, tableNames, rlsUsingExpression);
    }
}