using EShop.Shared.Diagnostics;
using EShop.Shared.DomainTools.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace EShop.Shared.JsonApi.Extensions;

public static class AspireConfigurationExtensions
{
    /// <summary>
    /// If the application is running in .NET Aspire, returns a connection string that is derived from the Aspire
    /// connection string, but with a database user that is constrained by the row-level security policy. Otherwise,
    /// returns the default connection string.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="aspireConnectionStringName"></param>
    /// <param name="environment"></param>
    /// <returns></returns>
    public static string GetRlsConnectionString(this IConfiguration configuration, string aspireConnectionStringName, IHostEnvironment environment)
    {
        if (configuration.IsRunningInAspire() && environment.IsDevelopment())
        {
            var defaultConnection = new Npgsql.NpgsqlConnectionStringBuilder(configuration.GetConnectionString("DefaultConnection"));

            var aspireConnectionString = configuration.GetConnectionString(aspireConnectionStringName).Require();
            var connection = new Npgsql.NpgsqlConnectionStringBuilder(aspireConnectionString)
            {
                Username = defaultConnection.Username,
                Password = defaultConnection.Password
            };

            return connection.ConnectionString;
        }

        return configuration.GetConnectionString("DefaultConnection").Require();
    }
}