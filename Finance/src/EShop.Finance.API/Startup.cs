using EShop.Finance.API.DependencyInjections;
using EShop.Finance.Application.DependencyInjection;
using EShop.Finance.Infrastructure.DependencyInjection;
using EShop.Shared.JsonApi.Middlewares;

namespace EShop.Finance.API;

public class Startup
{
    public IConfiguration Configuration { get; private set; }
    public IWebHostEnvironment Environment { get; private set; }

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
    }

    public virtual void ConfigureServices(IServiceCollection services)
    {
        services
            .AddFinanceAPI()
            .AddFinanceApplication()
            .AddFinancePersistence(Configuration, Environment)
            .AddFinanceInfrastructure(Configuration);
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

        app.MapDefaultEndpoints();
    }
}
