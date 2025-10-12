using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data;
using System.Data.Common;

namespace EShop.Testing.JsonApiApplication;

public sealed class PostgreSqlTestDatabaseConnectionInterceptor : DbConnectionInterceptor, ITestDatabaseConnectionInterceptor
{
    private readonly PostgreSqlTestDatabase _postgreSqlTestDatabase;

    public PostgreSqlTestDatabaseConnectionInterceptor(PostgreSqlTestDatabase postgreSqlTestDatabase)
    {
        _postgreSqlTestDatabase = postgreSqlTestDatabase;
    }

    public override InterceptionResult ConnectionOpening(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result)
    {
        if (eventData.Connection.State == ConnectionState.Open)
            return result;

        //_postgreSqlTestDatabase.SharedConnectionString.Require("PostgreSQL test database connection string should be set at this point.");

        eventData.Context?.Database.SetConnectionString(_postgreSqlTestDatabase.SharedConnectionString);
        return result;
    }

    public override ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Connection.State == ConnectionState.Open)
            return ValueTask.FromResult(result);

        eventData.Context?.Database.SetConnectionString(_postgreSqlTestDatabase.SharedConnectionString);
        return ValueTask.FromResult(result);
    }
}