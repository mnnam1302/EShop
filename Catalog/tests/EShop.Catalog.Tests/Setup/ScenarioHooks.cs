using EShop.Catalog.Application.Bootstrapping;
using EShop.Catalog.Application.Shared;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.DbResourceAccessControl;
using EShop.Shared.Sequences;
using EShop.Testing.JsonApiApplication;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Reqnroll;
using Reqnroll.Bindings;
using Reqnroll.BoDi;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;

namespace EShop.Catalog.Tests.Setup;

[Binding]
public sealed class ScenarioHooks
{
    private static PostgreSqlContainer PostgreSqlContainer = null!;
    private static MongoDbContainer MongoDbContainer = null!;

    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        try
        {
            PostgreSqlContainer = new PostgreSqlBuilder()
                    .WithPortBinding(36200, 5432)
                    .WithUsername("postgres")
                    .WithPassword("postgres")
                    .WithImage("postgres:17.0")
                    .Build();

            MongoDbContainer = new MongoDbBuilder()
                    .Build();

            await Task.WhenAll(
                PostgreSqlContainer.StartAsync(),
                MongoDbContainer.StartAsync());
        }
        catch (ArgumentException)
        {
            // Docker is not available — BDD scenario tests will fail individually,
            // but pure unit tests in the assembly should not be blocked.
        }
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

        var mongoDatabase = new MongoDbTestDatabase()
        {
            MongoDbContainer = MongoDbContainer
        };

        mongoDatabase.CreateDatabase();
        objectContainer.RegisterInstanceAs(mongoDatabase);

        var apiContext = new ApiContext(testDatabase, mongoDatabase);
        objectContainer.RegisterInstanceAs(apiContext);

        await InitializeDatabase(apiContext, testDatabase);
    }

    private static async Task InitializeDatabase(ApiContext apiContext, PostgreSqlTestDatabase testDatabase)
    {
        await using var scope = apiContext.ServiceProvider.CreateAsyncScope();
        await using var databaseConnection = new NpgsqlConnection(testDatabase.SharedConnectionString);
        await using var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        dbContext.Database.SetConnectionString(databaseConnection.ConnectionString);

        var dbInitilize = new DbInitializer(
            scope.ServiceProvider.GetRequiredService<ILogger<DbInitializer>>(),
            scope.ServiceProvider.GetRequiredService<IOptions<CatalogSequenceOptions>>(),
            dbContext,
            scope.ServiceProvider.GetRequiredService<ISequenceRegistry>(),
            scope.ServiceProvider.GetRequiredService<IUserDetailsProvider>(),
            scope.ServiceProvider.GetRequiredService<ITenantIsolationStrategy>());

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
    public async Task AfterScenario(PostgreSqlTestDatabase testDatabase, MongoDbTestDatabase mongoDatabase, ApiContext apiContext)
    {
        await apiContext.DisposeAsync();
        await testDatabase.DropAsync();
        await mongoDatabase.DropAsync();
    }

    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        if (PostgreSqlContainer is not null)
        {
            await PostgreSqlContainer.StopAsync();
            await PostgreSqlContainer.DisposeAsync();
        }

        if (MongoDbContainer is not null)
        {
            await MongoDbContainer.StopAsync();
            await MongoDbContainer.DisposeAsync();
        }
    }
}