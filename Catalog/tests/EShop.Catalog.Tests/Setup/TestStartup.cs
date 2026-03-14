using EShop.Catalog.Application.Agencies;
using EShop.Catalog.Application.Bootstrapping;
using EShop.Testing.JsonApiApplication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EShop.Catalog.Tests.Setup;

public sealed class TestStartup : Application.Startup
{
    private readonly PostgreSqlTestDatabase testDatabase;
    private readonly MongoDbTestDatabase mongoDatabase;

    public TestStartup(IConfiguration configuration, IWebHostEnvironment environment, PostgreSqlTestDatabase testDatabase, MongoDbTestDatabase mongoDatabase)
        : base(configuration, environment)
    {
        this.testDatabase = testDatabase;
        this.mongoDatabase = mongoDatabase;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddCatalogTestShared(Configuration, testDatabase, mongoDatabase)
            .AddCatalogTestBoostrapping()
            .AddAgencies();
    }

    public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
    {
        if (Environment.IsDevelopment())
        {
            app.UseCors(x => x.AllowAnyMethod());
        }

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapCatalogEndpoints();
        });
    }
}