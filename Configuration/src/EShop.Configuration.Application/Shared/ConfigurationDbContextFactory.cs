using EShop.Shared.DbResourceAccessControl.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EShop.Configuration.Application.Shared;

public class ConfigurationDbContextFactory : IDesignTimeDbContextFactory<ConfigurationDbContext>
{
    public ConfigurationDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = BuildConfiguration();

        var sqlVersionOptions = new NgSqlVersionOptions();
        configuration.GetSection(nameof(NgSqlVersionOptions)).Bind(sqlVersionOptions);

        var optionsBuilder = new DbContextOptionsBuilder<ConfigurationDbContext>();
        optionsBuilder
            .EnableDetailedErrors(true)
            .EnableSensitiveDataLogging(true)
            .UseLazyLoadingProxies(true)
            .UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptionsAction: options => options
                    .SetPostgresVersion(sqlVersionOptions.Major, sqlVersionOptions.Minor)
                    .MigrationsAssembly(typeof(ConfigurationDbContext).Assembly.GetName().Name));

        return new ConfigurationDbContext(optionsBuilder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var basePath = Directory.GetCurrentDirectory();
        Console.WriteLine($"Design-time configuration base path: {basePath}");

        return new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
