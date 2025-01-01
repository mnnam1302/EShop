using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Identity.Tests.Setups;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestBoostrapping(this IServiceCollection services, 
        IConfiguration configuration, 
        IWebHostEnvironment environment)
    {



        return services;
    }
}