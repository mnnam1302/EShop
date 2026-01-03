using EShop.Catalog.Application.Agencies;
using EShop.Catalog.Application.Bootstrapping;
using EShop.Catalog.Application.Shared;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.JsonApi.Middlewares;

namespace EShop.Catalog.Application;

public class Startup
{
    public IConfiguration Configuration { get; }
    public IWebHostEnvironment Environment { get; }

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
    }

    public virtual void ConfigureServices(IServiceCollection services)
    {
        services
            .AddShared(Configuration, Environment)
            .AddBoostrapping(Configuration, Environment)
            .AddAgencies();
    }

    internal void Configure(WebApplication app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<Startup>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (Environment.IsDevelopment() || Environment.IsStaging())
        {
            app.UseCors(x => x.AllowAnyMethod());
            app.UseSwaggerAPI();
        }

        app.UseRouting();
        app.MapCatalogEndpoints();

        app.RegisterFeatures(applicationLifetime, logger);
        app.RegisterPermissions(applicationLifetime, logger);
    }
}
