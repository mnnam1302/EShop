using EShop.Inventory.API.DependencyInjection;
using EShop.Inventory.Application.DependencyInjection;
using EShop.Inventory.Infrastructure.DependencyInjection;
using EShop.Shared.JsonApi.Middlewares;

namespace EShop.Inventory.API;

public class Startup(IConfiguration configuration, IWebHostEnvironment environment)
{
    public IConfiguration Configuration { get; } = configuration;
    public IWebHostEnvironment Environment { get; } = environment;

    public virtual void ConfigureServices(IServiceCollection services)
    {
        services
            .AddInventoryAPI()
            .AddInventoryApplication()
            .AddInventoryPersistence()
            .AddInventoryInfrastructure();
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

        app.UseRouting();
    }
}