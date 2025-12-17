using EShop.Catalog.ReadModels.MongoDb.Bootstrapping;
using EShop.Shared.JsonApi.Middlewares;
using JsonApiDotNetCore.Configuration;

namespace EShop.Catalog.ReadModels.MongoDb;

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
        app.UseJsonApi();
        app.MapControllers();
    }
}