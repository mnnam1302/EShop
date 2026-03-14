using EShop.Authorization.Infrastructure;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.DbResourceAccessControl;
using EShop.Testing.JsonApiApplication;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Reqnroll;
using Reqnroll.Bindings;
using Reqnroll.BoDi;
using Testcontainers.PostgreSql;

namespace EShop.Authorization.Tests.Setups;

[Binding]
public sealed class ScenarioHooks
{
    private static PostgreSqlContainer PostgreSqlContainer = null!;

    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        PostgreSqlContainer = new PostgreSqlBuilder()
                .WithPortBinding(36200, 5432)
                .WithUsername("postgres")
                .WithPassword("postgres")
                .WithImage("postgres:17.0")
                .Build();

        await PostgreSqlContainer.StartAsync();
    }

    [BeforeScenario]
    public async Task BeforeScenario(IObjectContainer objectContainer)
    {
        var testDatabase = new PostgreSqlTestDatabase()
        {
            PostgreSqlContainer = PostgreSqlContainer
        };

        await testDatabase.CreateSharedDatabaseAsync();
        objectContainer.RegisterInstanceAs<PostgreSqlTestDatabase>(testDatabase);

        var apiContext = new ApiContext(testDatabase);
        objectContainer.RegisterInstanceAs(apiContext);

        await InitializeDatabase(apiContext, testDatabase);
    }

    private static async Task InitializeDatabase(ApiContext apiContext, PostgreSqlTestDatabase testDatabase)
    {
        await using var scope = apiContext.ServiceProvider.CreateAsyncScope();
        await using var databaseConnection = new NpgsqlConnection(testDatabase.SharedConnectionString);
        await using var dbContext = scope.ServiceProvider.GetRequiredService<AuthorizationDbContext>();

        dbContext.Database.SetConnectionString(databaseConnection.ConnectionString);

        var dbInitilize = new DbInitializer(
            dbContext,
            scope.ServiceProvider.GetRequiredService<IUserDetailsProvider>(),
            scope.ServiceProvider.GetRequiredService<ITenantIsolationStrategy>(),
            scope.ServiceProvider.GetRequiredService<IConfiguration>(),
            scope.ServiceProvider.GetRequiredService<ILogger<DbInitializer>>());

        await dbInitilize.Initialize(applyMigrations: true, applyTenantIsolation: true);
    }

    [AfterStep]
    public async Task AfterStep(ScenarioContext scenarioContext, ApiContext apiContext)
    {
        if (scenarioContext.StepContext.StepInfo.StepDefinitionType is StepDefinitionType.Given or StepDefinitionType.When)
        {
            await apiContext.ConsumeObserver.WaitForQuietAsync();

            apiContext.LastApiError.Should().BeNull();
            apiContext.EventTracker.ClearPublishedEvents();
        }
    }

    [AfterScenario]
    public async Task AfterScenario(PostgreSqlTestDatabase testDatabase, ApiContext apiContext)
    {
        await apiContext.DisposeAsync();
        await testDatabase.DropAsync();
    }

    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        if (PostgreSqlContainer is not null)
        {
            await PostgreSqlContainer.StopAsync();
            await PostgreSqlContainer.DisposeAsync();
        }
    }
}
