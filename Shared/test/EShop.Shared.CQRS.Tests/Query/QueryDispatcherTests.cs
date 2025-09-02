using EShop.Shared.Contracts.Abstractions.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShop.Shared.CQRS.Tests.Query;

public class QueryDispatcherTests : TestBase
{
    private readonly Mock<ILogger<QueryDispatcher>> _loggerMock;

    public QueryDispatcherTests()
    {
        _loggerMock = CreateLoggerMock<QueryDispatcher>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        // Register query handlers
        services.AddScoped<IQueryHandler<TestQuery, TestResult>, TestQueryHandler>();

        // Register dispatcher with mocked logger
        services.AddScoped(_ => _loggerMock.Object);
        services.AddScoped<QueryDispatcher>();
    }

    [Fact]
    public async Task DispatchAsync_WithValidQuery_ShouldReturnSuccessWithResult()
    {
        // Arrange
        var queryId = Guid.NewGuid();
        var query = new TestQuery(queryId);
        var handler = GetService<IQueryHandler<TestQuery, TestResult>>();
        var dispatcher = GetService<QueryDispatcher>();

        // Act
        var result = await dispatcher.DispatchAsync<TestQuery, TestResult>(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(queryId);
        result.Value.Name.Should().Be("Test Item");
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var testHandler = handler as TestQueryHandler;
        testHandler.Should().NotBeNull();
        testHandler!.WasCalled.Should().BeTrue();
        testHandler.LastQuery.Should().Be(query);
    }

    [Fact]
    public async Task DispatchAsync_WithCancellationToken_ShouldPassTokenToHandler()
    {
        // Arrange
        var query = new TestQuery(Guid.NewGuid());
        var dispatcher = GetService<QueryDispatcher>();
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await dispatcher.Invoking(d => d.DispatchAsync<TestQuery, TestResult>(query, cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task DispatchAsync_WhenHandlerNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped(_ => _loggerMock.Object);
        services.AddScoped<QueryDispatcher>();

        using var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<QueryDispatcher>();
        var query = new TestQuery(Guid.NewGuid());

        // Act & Assert
        await dispatcher.Invoking(d => d.DispatchAsync<TestQuery, TestResult>(query))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DispatchAsync_WhenHandlerThrowsException_ShouldPropagateException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped(_ => _loggerMock.Object);
        services.AddScoped<QueryDispatcher>();
        services.AddScoped<IQueryHandler<TestQuery, TestResult>, ThrowingQueryHandler>();

        using var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<QueryDispatcher>();
        var query = new TestQuery(Guid.NewGuid());

        // Act & Assert
        await dispatcher.Invoking(d => d.DispatchAsync<TestQuery, TestResult>(query))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test query exception");
    }

    [Fact]
    public async Task DispatchAsync_WhenHandlerReturnsFailure_ShouldReturnFailure()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped(_ => _loggerMock.Object);
        services.AddScoped<QueryDispatcher>();
        services.AddScoped<IQueryHandler<TestQuery, TestResult>, FailingQueryHandler>();

        using var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<QueryDispatcher>();
        var query = new TestQuery(Guid.NewGuid());

        // Act
        var result = await dispatcher.DispatchAsync<TestQuery, TestResult>(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Query.Failed");
        result.Error.Message.Should().Be("This query handler always fails");
    }

    [Fact]
    public void DispatchAsync_GenericOverload_ShouldMaintainTypeConsistency()
    {
        // Arrange & Act
        var dispatcher = GetService<QueryDispatcher>();
        var query = new TestQuery(Guid.NewGuid());

        // Assert - This should compile and maintain type safety
        Func<Task<Result<TestResult>>> action = () => dispatcher.DispatchAsync<TestQuery, TestResult>(query);
        action.Should().NotBeNull();
    }
}
