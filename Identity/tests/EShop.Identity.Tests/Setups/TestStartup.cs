using EShop.Identity.API.DependencyInjections.Extensions;
using EShop.Shared.JsonApi.Middlewares;
using EShop.Testing.JsonApiApplication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Research run parallel testin with Xunit and Reqnroll: https://docs.reqnroll.net/latest/execution/parallel-execution.html
namespace EShop.Identity.Tests.Setups;

public class TestStartup : Identity.API.Startup
{
    private readonly PostgreSqlTestDatabase _testDatabase;

    public TestStartup(IConfiguration configuration, IWebHostEnvironment env, PostgreSqlTestDatabase testDatabase)
        : base(configuration, env)
    {
        this.Environment.EnvironmentName = "Development";
        _testDatabase = testDatabase;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddTestShared(Configuration, Environment, _testDatabase)
            .AddTestBoostrapping(Configuration, Environment);
    }

    public override void Configure(
        WebApplication app,
        IHostApplicationLifetime applicationLifetime,
        ILoggerFactory loggerFactory)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (Environment.IsDevelopment())
        {
            app.UseCors(x => x.AllowAnyMethod());
            app.UseSwaggerAPI();
        }

        app.UseRouting();
        app.MapControllers();
    }
}