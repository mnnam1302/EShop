using EShop.Shared.RateLimiting.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EShop.Shared.RateLimiting.Tests.AspNetCore;

// Builds a real IOptionsMonitor<RateLimiterEnforcementOptions> backed by a mutable ConfigurationManager,
// so tests can flip a flag at runtime and observe CurrentValue change — the same mechanism a live
// appsettings.json reload uses in production, just without a file on disk.
internal static class EnforcementOptionsTestFactory
{
    public static (ConfigurationManager Configuration, IOptionsMonitor<RateLimiterEnforcementOptions> Monitor) Create(
        bool tenantEnforced = false,
        bool userEnforced = false,
        bool anonymousIpEnforced = false)
    {
        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"{RateLimiterEnforcementOptions.SectionName}:TenantEnforced"] = tenantEnforced.ToString(),
            [$"{RateLimiterEnforcementOptions.SectionName}:UserEnforced"] = userEnforced.ToString(),
            [$"{RateLimiterEnforcementOptions.SectionName}:AnonymousIpEnforced"] = anonymousIpEnforced.ToString()
        });

        var services = new ServiceCollection();
        services.AddOptions<RateLimiterEnforcementOptions>().Bind(configuration.GetSection(RateLimiterEnforcementOptions.SectionName));
        var provider = services.BuildServiceProvider();

        return (configuration, provider.GetRequiredService<IOptionsMonitor<RateLimiterEnforcementOptions>>());
    }

    // Mutating a ConfigurationManager value doesn't synchronously rebind IOptionsMonitor's
    // CurrentValue — the rebind happens via a reload-token callback, same as it would against a real
    // appsettings.json reload. Waiting for OnChange makes the test deterministic instead of racing it.
    public static async Task SetFlagAsync(
        ConfigurationManager configuration,
        IOptionsMonitor<RateLimiterEnforcementOptions> monitor,
        string propertyName,
        bool value)
    {
        var changed = new TaskCompletionSource();
        using var registration = monitor.OnChange(_ => changed.TrySetResult());

        configuration[$"{RateLimiterEnforcementOptions.SectionName}:{propertyName}"] = value.ToString();

        // MemoryConfigurationProvider.Set(...) mutates its backing dictionary without raising a
        // reload notification on its own; an explicit Reload() is what actually fires the change
        // token IOptionsMonitor is subscribed to (mirrors what a real file-watcher-driven
        // appsettings.json reload does under the hood).
        ((IConfigurationRoot)configuration).Reload();

        await changed.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }
}
