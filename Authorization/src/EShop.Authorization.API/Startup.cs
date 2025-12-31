using EShop.Authorization.API.APIs;
using EShop.Authorization.API.Boostrapping;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.JsonApi.Middlewares;

namespace EShop.Authorization.API;

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
        services.AddBoostrapping(Configuration, Environment);
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

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapAuthorizationEndpoints();

        app.RegisterFeatures(applicationLifetime, logger);
        app.RegisterPermissions(applicationLifetime, logger);
    }
}