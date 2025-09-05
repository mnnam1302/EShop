using EShop.Configuration.Application.Agencies;
using EShop.Configuration.Application.Boostrapping;
using EShop.Configuration.Application.Shared;
using EShop.Shared.JsonApi.DependencyInjections;
using EShop.Shared.JsonApi.Middlewares;

namespace EShop.Configuration.Application;

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
            .AddShared(Configuration)
            .AddBoostrapping(Configuration, Environment)
            .AddAgencies();
    }

    public virtual void Configure(WebApplication app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<Startup>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (Environment.IsDevelopment() || Environment.IsStaging())
        {
            app.UseCors(x => x.AllowAnyMethod());
            app.UseSwaggerAPI();
        }

        app.UseRouting();
        app.MapConfigurationEndpoints();

        app.RegisterFeatures(applicationLifetime, logger);
        app.RegisterPermissions(applicationLifetime, logger);
    }
}