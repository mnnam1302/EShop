using EShop.Tenancy.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EShop.Tenancy.Infrastructure.Jobs;

internal sealed class SystemInitializationJob(
    ILogger<SystemInitializationJob> logger,
    IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation("Starting system initialization...");

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            var scope = serviceScopeFactory.CreateScope();

            var initializationHandler = scope.ServiceProvider.GetRequiredService<ISystemInitializationService>();
            await initializationHandler.InitializeSystemAsync(stoppingToken);

            logger.LogInformation("System initialization completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize system");
        }
    }
}
