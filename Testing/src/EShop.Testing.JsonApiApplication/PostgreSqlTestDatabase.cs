using Npgsql;
using System.Collections.Concurrent;
using Testcontainers.PostgreSql;

namespace EShop.Testing.JsonApiApplication
{
    public sealed class PostgreSqlTestDatabase
    {
        private readonly ConcurrentDictionary<string, string> tenantSpecificConnectionStrings = new();

        public required PostgreSqlContainer PostgreSqlContainer { get; set; }

        public string SharedConnectionString { get; private set; }

        /// <summary>
        /// Call this method in Specflow's [BeforeScenario] hook to create and maintain a single
        /// PostgreSQL shared database for the entire scenario.
        /// </summary>
        /// <param name="databaseName">The database name; if not specified, the database will be created
        /// with a random name.</param>
        /// <param name="onDatabaseCreated">Called after the database was created.</param>
        /// <returns>The connection string to the shared database.</returns>
        public async Task<string> CreateSharedDatabaseAsync(
            string? databaseName = null,
            PostgreSqlTestDatabaseCreated? onDatabaseCreated = null)
        {
            this.SharedConnectionString = await CreateDatabaseAsync(
                databaseName: databaseName ?? Guid.NewGuid().ToString(),
                onDatabaseCreated);

            return this.SharedConnectionString;
        }

        /// <summary>
        /// Creates a tenant-specific PostgreSQL database for the specified <paramref name="tenantId"/>.
        /// Use this method for scenarios that test database routing.
        /// </summary>
        /// <param name="tenantId">The tenant ID to create a tenant-specific database for.</param>
        /// <param name="databaseName">The database name; if not specified, <paramref name="tenantId"/>
        /// will be used as the database name.</param>
        /// <param name="onDatabaseCreated">Called after the database was created.</param>
        /// <returns>The connection string to the tenant-specific database.</returns>
        public async Task<string> CreateForTenantAsync(
            string tenantId,
            string? databaseName = null,
            PostgreSqlTestDatabaseCreated? onDatabaseCreated = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
            if (databaseName is not null && string.IsNullOrWhiteSpace(databaseName))
            {
                throw new ArgumentException("Database name cannot be empty.", nameof(databaseName));
            }

            var connectionString = await CreateDatabaseAsync(
                databaseName: databaseName ?? tenantId,
                onDatabaseCreated);

            if (!this.tenantSpecificConnectionStrings.TryAdd(tenantId, connectionString))
            {
                throw new InvalidOperationException($"Duplicated tenant-specific database for tenant {tenantId}.");
            }

            return connectionString;
        }

        private async Task<string> CreateDatabaseAsync(string databaseName, PostgreSqlTestDatabaseCreated? onDatabaseCreated)
        {
            var masterConnectionString = PostgreSqlContainer.GetConnectionString();
            var username = new NpgsqlConnectionStringBuilder(masterConnectionString).Username;

            var createDatabaseScript =
                $"""
                CREATE DATABASE "{databaseName}";
                GRANT ALL PRIVILEGES ON DATABASE "{databaseName}" TO {username};
                """;

            await PostgreSqlContainer.ExecScriptAsync(createDatabaseScript);

            var databaseConnectionString = new NpgsqlConnectionStringBuilder(masterConnectionString)
            {
                Database = databaseName,
                IncludeErrorDetail = true
            }.ConnectionString;

            if (onDatabaseCreated is not null)
            {
                await onDatabaseCreated.Invoke(databaseName, databaseConnectionString, PostgreSqlContainer);
            }

            return databaseConnectionString;
        }

        /// <summary>
        /// Gets the connection string for the specified <paramref name="tenantId"/>. If the tenant-specific
        /// connection string is not found, the shared connection string is returned so that existing BDD tests
        /// still work without any modifications.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>A connection string.</returns>
        public string GetConnectionString(string tenantId)
        {
            if (this.tenantSpecificConnectionStrings.TryGetValue(tenantId, out var tenantSpecificConnectionString))
            {
                return tenantSpecificConnectionString;
            }

            return this.SharedConnectionString;
        }

        /// <summary>
        /// Call this method in Specflow's [AfterScenario] hook to drop PostgreSQL databases.
        /// </summary>
        public async Task DropAsync()
        {
            var databaseNames = new[] { this.SharedConnectionString }
                .Concat(this.tenantSpecificConnectionStrings.Values)
                .Where(conn => conn is not null)
                .Select(conn => new NpgsqlConnectionStringBuilder(conn).Database)
                .Distinct();

            foreach (var databaseName in databaseNames)
            {
                var sqlScript =
                    $"""
                -- Revoke the CONNECT privilege on the specified database from all users (PUBLIC),
                -- which prevents any new connections to the database.
                REVOKE CONNECT ON DATABASE "{databaseName}" FROM PUBLIC;

                -- Terminate any existing connections to the database except the connection executing this script.
                SELECT pg_terminate_backend(pg_stat_activity.pid)
                  FROM pg_stat_activity
                  WHERE pg_stat_activity.datname = '{databaseName}' AND pid <> pg_backend_pid();

                DROP DATABASE IF EXISTS "{databaseName}";
                """;

                await this.PostgreSqlContainer.ExecScriptAsync(sqlScript);
            }
        }
    }
}

public delegate Task PostgreSqlTestDatabaseCreated(
    string databaseName,
    string connectionString,
    PostgreSqlContainer postgreSqlContainer);