using EShop.Shared.CQRS.Command;
using EShop.Shared.CQRS.Tests.TestHelpers;
using EShop.Shared.Contracts.Abstractions.Shared;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace EShop.Shared.CQRS.Tests.Command;

public class CommandDispatcherTests : TestBase
{
    private readonly Mock<ILogger<CommandDispatcher>> _loggerMock;

    public CommandDispatcherTests()
    {
        _loggerMock = CreateLoggerMock<CommandDispatcher>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        
        // Register command handlers
        services.AddScoped<ICommandHandler<TestCommand>, TestCommandHandler>();
        services.AddScoped<ICommandHandler<TestCommandWithResult, TestResult>, TestCommandWithResultHandler>();
        
        // Register dispatcher with mocked logger
        services.AddScoped(_ => _loggerMock.Object);
        services.AddScoped<CommandDispatcher>();
    }

    [Fact]
    public async Task DispatchAsync_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = new TestCommand("Test Command");
        var handler = GetService<ICommandHandler<TestCommand>>();
        var dispatcher = GetService<CommandDispatcher>();

        // Act
        var result = await dispatcher.DispatchAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        var testHandler = handler as TestCommandHandler;
        testHandler.Should().NotBeNull();
        testHandler!.WasCalled.Should().BeTrue();
        testHandler.LastCommand.Should().Be(command);
    }

    [Fact]
    public async Task DispatchAsync_WithValidCommandWithResult_ShouldReturnSuccessWithResult()
    {
        // Arrange
        var command = new TestCommandWithResult("Test Command With Result");
        var dispatcher = GetService<CommandDispatcher>();

        // Act
        var result = await dispatcher.DispatchAsync<TestCommandWithResult, TestResult>(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(command.Name);
        result.Value.Id.Should().NotBeEmpty();
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DispatchAsync_WithCancellationToken_ShouldPassTokenToHandler()
    {
        // Arrange
        var command = new TestCommand("Test Command");
        var dispatcher = GetService<CommandDispatcher>();
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await dispatcher.Invoking(d => d.DispatchAsync(command, cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task DispatchAsync_WhenHandlerNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped(_ => _loggerMock.Object);
        services.AddScoped<CommandDispatcher>();
        
        using var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<CommandDispatcher>();
        var command = new TestCommand("Test Command");

        // Act & Assert
        await dispatcher.Invoking(d => d.DispatchAsync(command))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DispatchAsync_WhenHandlerThrowsException_ShouldPropagateException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped(_ => _loggerMock.Object);
        services.AddScoped<CommandDispatcher>();
        services.AddScoped<ICommandHandler<TestCommand>, ThrowingCommandHandler>();
        
        using var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<CommandDispatcher>();
        var command = new TestCommand("Test Command");

        // Act & Assert
        await dispatcher.Invoking(d => d.DispatchAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");
    }

    [Fact]
    public async Task DispatchAsync_WhenHandlerReturnsFailure_ShouldReturnFailure()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped(_ => _loggerMock.Object);
        services.AddScoped<CommandDispatcher>();
        services.AddScoped<ICommandHandler<TestCommand>, FailingCommandHandler>();
        
        using var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<CommandDispatcher>();
        var command = new TestCommand("Test Command");

        // Act
        var result = await dispatcher.DispatchAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Test.Failed");
        result.Error.Message.Should().Be("This command handler always fails");
    }

    [Fact]
    public async Task DispatchAsync_ShouldLogExecutionDetails()
    {
        // Arrange
        var command = new TestCommand("Test Command");
        var dispatcher = GetService<CommandDispatcher>();

        // Act
        await dispatcher.DispatchAsync(command);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Dispatching command") && v.ToString()!.Contains("TestCommand")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Command") && v.ToString()!.Contains("handled successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_WhenHandlerFails_ShouldLogWarning()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped(_ => _loggerMock.Object);
        services.AddScoped<CommandDispatcher>();
        services.AddScoped<ICommandHandler<TestCommand>, FailingCommandHandler>();
        
        using var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<CommandDispatcher>();
        var command = new TestCommand("Test Command");

        // Act
        await dispatcher.DispatchAsync(command);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed") && v.ToString()!.Contains("TestCommand")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
