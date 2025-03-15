using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nito.AsyncEx;

namespace EShop.Shared.HealthChecks;

public static class HealthCheckExtentions
{
    public static void WaitForHealthyEventBus(this HealthCheckService healthCheckService)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        HealthReport result;
        do
        {
            result = AsyncContext.Run<HealthReport>(() => WaitAndCheckMassTransitAsync(healthCheckService, cts));
        }
        while (result.Status != HealthStatus.Healthy);
    }

    private static async Task<HealthReport> WaitAndCheckMassTransitAsync(HealthCheckService healthCheckService, CancellationTokenSource cts)
    {
        const int DelayInMs = 1000;
        await Task.Delay(DelayInMs, cts.Token);
        return await healthCheckService.CheckHealthAsync(x => x.Tags.Contains("masstransit"), cts.Token);
    }
}