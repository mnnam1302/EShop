using EShop.Identity.API.DependencyInjections.Extensions;
using EShop.Shared.JsonApi.Middlewares;
using EShop.Shared.Scoping.DependencyInjections.Extensions;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace EShop.Identity.API;

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
            .AddBoostrapping(Configuration, Environment);
    }

    public virtual void Configure(WebApplication app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<Startup>();

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (Environment.IsDevelopment())
        {
            app.UseCors(x => x.AllowAnyMethod());
            app.UseSwaggerAPI();
        }

        app.UseRouting();
        app.MapHealthChecks("/_health",
            new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.RegisterFeatures(applicationLifetime, logger);
    }
}