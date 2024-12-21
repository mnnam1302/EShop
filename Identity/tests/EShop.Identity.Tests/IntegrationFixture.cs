using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace EShop.Identity.Tests;

public class IntegrationFixture : IAsyncLifetime
{
    // class fixture: when use it: when you want to create a single test context and share it among all the tests in the class, and have it cleaned up after all the tests in the class have finished.
    // https://xunit.net/docs/shared-context#class-fixture

    private readonly PostgreSqlContainer _postgreSqlContainer;

    public IntegrationFixture()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithDatabase("identity_postgres_test")
            .WithPortBinding(49111, 5432)
            .WithUsername("postgres")
            .WithPassword("password")
            .WithImage("postgres:17.0")
            .Build();
    }

    public MockApp App { get; set; }
    public HttpClient Client { get; set; }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        App = new MockApp(_postgreSqlContainer.GetConnectionString());
        Client = App.CreateClient();
    }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    public class MockApp : WebApplicationFactory<Identity.API.Program>
    {
        private readonly string _postgresConnection;

        public MockApp(string postgresConnection)
        {
            this._postgresConnection = postgresConnection;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                //services
            });
        }
    }
}

[CollectionDefinition(nameof(IntegrationFixtureCollection))]
public class IntegrationFixtureCollection : ICollectionFixture<IntegrationFixture>
{
    /*
     * collection fixture: when use it: when you want to create a single test context and share it among tests in several test classes, and have it cleaned up after all the tests in the test classes have finished.
     * recieve an instance of fixture data IntegrationFixture, which is initialized before the first tests run, and disposed after the last test in the collection run
     *
     * a constructor argument of type IntegrationFixture is required for the IntegrationTest class
     */
}

[Collection(nameof(IntegrationFixtureCollection))]
public class IntegrationTest : IAsyncLifetime
{
    public IntegrationTest(IntegrationFixture integrationFixture)
    {
        IntegrationFixture = integrationFixture;
    }

    public IntegrationFixture IntegrationFixture { get; }
    public HttpClient Client => IntegrationFixture.Client;
    public IServiceScope Scope { get; private set; }
    public IServiceProvider Services => Scope.ServiceProvider;

    public Task InitializeAsync()
    {
        Scope = IntegrationFixture.App.Services.CreateScope();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Scope.Dispose();
        return Task.CompletedTask;
    }
}