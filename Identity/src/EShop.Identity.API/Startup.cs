using EShop.Identity.Persistence;
using EShop.Shared.JsonApi.DependencyInjections;
using Serilog;

namespace EShop.Identity.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Shared - Common
            services.AddCors();

            services.AddDbContextWithScoping<UserDbContext>(Configuration, false);
            services.AddTransient<DbInitializer>();

            /*
             * API
             * - Controllers
             * - Api Versioning
             * - Swagger
             * - Health Checks
             * - Logger - later bring to shared folder
             */

            /*
             * Application
             * - Automapper
             * - MediatR
             */

            // Persistence

            // Infrastructure
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
        {
        }
    }
}
