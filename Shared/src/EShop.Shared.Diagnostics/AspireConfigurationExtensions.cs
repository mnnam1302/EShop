using Microsoft.Extensions.Configuration;

namespace EShop.Shared.Diagnostics;

public static class AspireConfigurationExtensions
{
    /// <summary>
    /// Gets a value indicating whether the application is running in .NET Aspire.
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static bool IsRunningInAspire(this IConfiguration configuration)
    {
        return configuration.GetValue("runInAspire", false);
    }
}