using EShop.Shared.Diagnostics;
using EShop.Shared.JsonApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace EShop.Finance.Infrastructure.DependencyInjection;

internal static class ConnectionStringExtensions
{
    internal static string GetConnectionString(this IConfiguration configuration, IHostEnvironment environment)
    {
        return configuration.GetRlsConnectionString(configuration.GetConnectionStringName(), environment);
    }

    internal static string GetConnectionStringName(this IConfiguration configuration)
    {
        return configuration.IsRunningInAspire() ? "financeDatabase" : "DefaultConnection";
    }
}
