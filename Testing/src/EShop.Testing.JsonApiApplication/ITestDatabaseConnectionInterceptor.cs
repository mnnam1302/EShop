using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EShop.Testing.JsonApiApplication;

/// <summary>
/// Intercepts the <see cref="DbContext"/>'s connection and sets the connection string to the shared test database.
/// </summary>
/// <remarks>
/// If the BDD test project uses <see cref="TestDatabase"/> to manage the test database, the connection string
/// to the test database is not available until the test database is created at runtime (see <see cref="TestDatabase.CreateSharedDatabase"/>).
/// As a result, the test <see cref="DbContext"/> cannot be configured with the connection string at design time see
/// <see cref="DependencyInjectionExtensions.AddTestDbContext{TContext}"/> and <see cref="DependencyInjectionExtensions.AddTestDbContextFactory{TContext}"/>.
/// This interceptor sets the <see cref="DbContext"/>'s connection string to the shared test database before the connection is opened.
/// </remarks>
public interface ITestDatabaseConnectionInterceptor : IDbConnectionInterceptor
{
}

/// <summary>
/// Implemented with SqlLite connection later
/// </summary>
public sealed class TestDatabaseConnectionInterceptor : DbConnectionInterceptor, ITestDatabaseConnectionInterceptor
{

}