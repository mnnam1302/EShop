using EShop.Shared.JsonApi.Middlewares;
using EShop.Tenancy.API.DependencyInjections.Extensions;

namespace EShop.Tenancy.API;

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

    public virtual void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();


        if (Environment.IsDevelopment() || Environment.IsStaging())
        {
            //app.UseCors(x => x.AllowAnyMethod());
        }
    }
}