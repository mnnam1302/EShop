using EShop.Inventory.API.DependencyInjection;
using EShop.Inventory.Application.DependencyInjection;
using EShop.Inventory.Infrastructure.DependencyInjection;
using EShop.Shared.JsonApi.Middlewares;

namespace EShop.Inventory.API;

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
            .AddInventoryAPI()
            .AddInventoryApplication()
            .AddInventoryPersistence(Configuration, Environment)
            .AddInventoryInfrastructure(Configuration);
    }

    public void Configure(WebApplication app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<Startup>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (Environment.IsDevelopment() || Environment.IsStaging())
        {
            app.UseCors(x => x.AllowAnyMethod());
            app.UseSwaggerAPI();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapEndpoints();

        app.UseRouting();
    }
}
