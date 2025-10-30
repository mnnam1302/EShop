using Carter;
using EShop.Tenancy.API.DependencyInjections.Extensions;
using EShop.Testing.JsonApiApplication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EShop.Tenancy.Tests.Setups;

public sealed class TestStartup : API.Startup
{
    private readonly PostgreSqlTestDatabase testDatabase;

    public TestStartup(IConfiguration configuration, IWebHostEnvironment environment, PostgreSqlTestDatabase testDatabase)
        : base(configuration, environment)
    {
        this.testDatabase = testDatabase;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTestShared(testDatabase);
        services.AddTestBoostrapping();
    }

    public override void Configure(WebApplication app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
    {
        if (Environment.IsDevelopment())
        {
            app.UseCors(x => x.AllowAnyMethod());
            app.UseSwaggerAPI();
        }

        app.UseRouting();
        app.MapCarter();
    }
}
