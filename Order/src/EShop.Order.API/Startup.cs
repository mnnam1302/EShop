using EShop.Order.API.DependencyInjection;
using EShop.Order.Application.DependencyInjections;
using EShop.Order.Infrastructure.DependencyInjection;
using EShop.Shared.JsonApi.Middlewares;

namespace EShop.Order.API;

public class Startup(IConfiguration configuration, IWebHostEnvironment environment)
{
    public IConfiguration Configuration { get; set; } = configuration;
    public IWebHostEnvironment Environment { get; set; } = environment;

    public virtual void ConfigureServices(IServiceCollection services)
    {
        services
            .AddOrderAPI()
            .AddOrderApplication()
            .AddOrderPersistence(Configuration, Environment)
            .AddOrderInfrastructure();
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

        app.MapEndpoints();
    }
}
