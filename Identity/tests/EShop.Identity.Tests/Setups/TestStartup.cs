using EShop.Identity.API.DependencyInjections.Extensions;
using EShop.Testing.JsonApiApplication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Research run parallel testing with XUnit and Reqnroll: https://docs.reqnroll.net/latest/execution/parallel-execution.html
namespace EShop.Identity.Tests.Setups;

public class TestStartup : Identity.API.Startup
{
    private readonly PostgreSqlTestDatabase _testDatabase;

    public TestStartup(IConfiguration configuration, IWebHostEnvironment environment, PostgreSqlTestDatabase testDatabase)
        : base(configuration, environment)
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

    public void Configure(
        IApplicationBuilder app,
        IHostApplicationLifetime applicationLifetime,
        ILoggerFactory loggerFactory)
    {
        // I think that shouldn't add exception handling middleware in test project because we can take system and application error to investigate
        //app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (Environment.IsDevelopment())
        {
            app.UseCors(x => x.AllowAnyMethod());
            app.UseSwaggerAPI();
        }

        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}