using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShop.Shared.CQRS.Tests.TestHelpers;

/// <summary>
/// Base class for tests providing common setup and utilities
/// </summary>
public abstract class TestBase : IDisposable
{
    protected ServiceProvider ServiceProvider { get; }
    protected IServiceCollection Services { get; }
    protected Mock<ILogger<T>> CreateLoggerMock<T>() => new();

    protected TestBase()
    {
        Services = new ServiceCollection();
        ConfigureServices(Services);
        ServiceProvider = Services.BuildServiceProvider();
    }

    /// <summary>
    /// Override this method to configure services for the test
    /// </summary>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Add logging
        services.AddLogging();
    }

    /// <summary>
    /// Get a service from the DI container
    /// </summary>
    protected T GetService<T>() where T : notnull => ServiceProvider.GetRequiredService<T>();

    public virtual void Dispose()
    {
        ServiceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}
