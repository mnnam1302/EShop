using EShop.Testing.JsonApiApplication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Identity.Tests.Setups;

public class TestStartup : Identity.API.Startup
{
    private readonly PostgreSqlTestDatabase _testDatabase;

    public TestStartup(IConfiguration configuration, IWebHostEnvironment env, PostgreSqlTestDatabase testDatabase)
        : base(configuration, env)
    {
        _testDatabase = testDatabase;
    }

    public override void ConfigureServices(IServiceCollection services)
    {

    }
}