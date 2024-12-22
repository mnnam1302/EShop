using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace EShop.Testing.JsonApiApplication;

public static class PostgreSqlTestDatabaseInitializer
{
    public static void Initialize<TContext>(
        IServiceProvider serviceProvider,
        string connectionString,
        bool applyTenantIsolation = true)
        where TContext : DbContext
    {
        using var scope = serviceProvider.CreateScope();
        using var databaseConnection = new NpgsqlConnection(connectionString);
        using var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        //if (dbContext is )
    }
}