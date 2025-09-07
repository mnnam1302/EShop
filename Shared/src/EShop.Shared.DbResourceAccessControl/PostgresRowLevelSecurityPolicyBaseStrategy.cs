using EShop.Shared.DbResourceAccessControl.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using NpgsqlTypes;
using System.Data.Common;

namespace EShop.Shared.DbResourceAccessControl;

public abstract class PostgresRowLevelSecurityPolicyBaseStrategy
{
    private readonly string policyName;
    private readonly ILogger logger;

    public PostgresRowLevelSecurityPolicyBaseStrategy(string policyName, ILogger logger)
    {
        this.policyName = policyName;
        this.logger = logger;
    }

    /// <summary>
    /// Indicates whether to adjust (i.e. ALTER POLICY) RLS policies on startup.
    /// </summary>
    protected abstract bool AdjustRlsPolicyOnStartUp { get; }

    protected void AddIsolationStrategy(DbContext dbContext, string[] tableNames, string rlsUsingExpression)
    {
        dbContext.Database.OpenConnection();

        // Do not dispose this connection as it will be used for the duration of DI scope
        var dbConnection = dbContext.Database.GetDbConnection();

        GetConfiguredSchema(dbContext, out var schemaName, out var schemaPrefix);

        logger.LogDebug(
            "Initializing Postgres RLS Policy '{policy}' for tables {tables} in schema '{schema}'",
            policyName, tableNames, schemaName);

        foreach (var table in tableNames)
        {
            var policyExists = DoesPolicyExists(dbConnection, schemaName, table);
            if (!policyExists)
            {
                AddRlsPolicy(dbContext, schemaPrefix, table, rlsUsingExpression);
            }
            else if (AdjustRlsPolicyOnStartUp)
            {
                AlterRlsPolicy(dbContext, schemaPrefix, table, rlsUsingExpression);
            }
            else
            {
                logger.LogDebug(
                    "Skipped adding or altering RLS Policy '{policy}' for table '{schemaPrefix}{table}' as it already exists and the current setting does not allow re-creation",
                    policyName, schemaPrefix, table);
            }
        }
    }

    private static void GetConfiguredSchema(DbContext dbContext, out string schemaName, out string schemaPrefix)
    {
        var builder = new DbConnectionStringBuilder()
        {
            ConnectionString = dbContext.Database.GetDbConnection().ConnectionString
        };
        bool isUsingSchema = builder.TryGetValue("Search Path", out var schema);
        schemaName = isUsingSchema ? schema?.ToString() ?? string.Empty : string.Empty;
        schemaPrefix = isUsingSchema && !string.IsNullOrWhiteSpace(schemaName) ? $"{schemaName}." : string.Empty;
    }

    private bool DoesPolicyExists(DbConnection dbConnection, string schemaName, string table)
    {
        DbCommand cmd;
        if (string.IsNullOrEmpty(schemaName))
        {
            cmd = dbConnection.CreateCommand()
                .WithPostgreSqlCommandText("SELECT count(*) FROM pg_policies WHERE tablename = $1 AND policyname = $2 LIMIT 1;")
                .WithPostgreSqlPositionalParameter(table, NpgsqlDbType.Varchar)
                .WithPostgreSqlPositionalParameter(policyName, NpgsqlDbType.Varchar);
        }
        else
        {
            cmd = dbConnection.CreateCommand()
                .WithPostgreSqlCommandText("SELECT count(*) FROM pg_policies WHERE schemaname = $1 AND tablename = $2 AND policyname = $3 LIMIT 1;")
                .WithPostgreSqlPositionalParameter(schemaName, NpgsqlDbType.Varchar)
                .WithPostgreSqlPositionalParameter(table, NpgsqlDbType.Varchar)
                .WithPostgreSqlPositionalParameter(policyName, NpgsqlDbType.Varchar);
        }

        // See https://www.npgsql.org/doc/prepare.html
        cmd.Prepare();

        var count = (long?)cmd.ExecuteScalar();
        return count != 0;
    }

    private void AddRlsPolicy(DbContext dbContext, string schemaPrefix, string table, string rlsUsingExpression)
    {
        logger.LogDebug(
            "Setting up RLS Policy '{policy}' for table '{schemaPrefix}{table}' using expression {rlsUsingExpression}",
            policyName, schemaPrefix, table, rlsUsingExpression);

        var enableRlsSql = $"ALTER TABLE {schemaPrefix}\"{table}\" ENABLE ROW LEVEL SECURITY;";
        dbContext.Database.ExecuteSqlRaw(enableRlsSql);

        var forceRlsSql = $"ALTER TABLE {schemaPrefix}\"{table}\" FORCE ROW LEVEL SECURITY;";
        dbContext.Database.ExecuteSqlRaw(forceRlsSql);

        var createPolicySql = $"CREATE POLICY {policyName} ON {schemaPrefix}\"{table}\" USING ({rlsUsingExpression});";
        dbContext.Database.ExecuteSqlRaw(createPolicySql);
    }

    private void AlterRlsPolicy(DbContext dbContext, string schemaPrefix, string table, string rlsUsingExpression)
    {
        logger.LogDebug(
            "Altering existing RLS Policy '{policy}' for table '{schemaPrefix}{table}' using expression {rlsUsingExpression}",
            policyName, schemaPrefix, table, rlsUsingExpression);

        var alterPolicySql =
            $"""
                 ALTER POLICY {policyName} ON {schemaPrefix}"{table}"
                     USING ({rlsUsingExpression});
                 """;
        dbContext.Database.ExecuteSqlRaw(alterPolicySql);
    }

    protected static string[] GetTableNamesForEntitiesFoundIn(DbContext dbContext, Func<IEntityType, bool> filter)
    {
        return dbContext.Model.GetEntityTypes()
            .Where(filter)
            .Select(entityType => entityType.GetTableName())
            .Where(tableName => tableName is not null)
            .Select(tableName => tableName!)
            .ToArray();
    }
}