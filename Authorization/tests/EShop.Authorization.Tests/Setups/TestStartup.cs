using EShop.Authorization.API.APIs;
using EShop.Testing.JsonApiApplication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EShop.Authorization.Tests.Setups;

public class TestStartup : Authorization.API.Startup
{
    private readonly PostgreSqlTestDatabase testDatabase;

    public TestStartup(IConfiguration configuration, IWebHostEnvironment environment, PostgreSqlTestDatabase testDatabase)
        : base(configuration, environment)
    {
        this.testDatabase = testDatabase;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddTestShared()
            .AddTestBoostrapping(this.testDatabase);
    }

    public override void Configure(WebApplication app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
    {
        if (Environment.IsDevelopment())
        {
            app.UseCors(x => x.AllowAnyMethod());
        }

        app.UseRouting();
        app.MapAuthenticationEndpoints();
    }
}
