using EShop.Catalog.SyncService.MongoDb.Boostrapping;
using EShop.Shared.JsonApi.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace EShop.Catalog.SyncService.MongoDb;

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
        services.AddShared();
        services.AddBoostrapping(Configuration, Environment);
    }

    internal void Configure(WebApplication app, IHostApplicationLifetime applicationLifetime)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (Environment.IsDevelopment() || Environment.IsStaging())
        {
            app.UseCors(x => x.AllowAnyMethod());
            app.UseSwaggerAPI();
        }

        app.UseRouting();
    }
}

