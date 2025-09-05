using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EShop.Shared.CQRS.Tests;

public class MediatorTests : TestBase
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddCQRS(Assembly.GetExecutingAssembly());

        // Register test handlers
        services.AddScoped<ICommandHandler<TestCommand>, TestCommandHandler>();
        services.AddScoped<ICommandHandler<TestCommandWithResult, TestResult>, TestCommandWithResultHandler>();
        services.AddScoped<IQueryHandler<TestQuery, TestResult>, TestQueryHandler>();
    }

    [Fact]
    public async Task SendAsync_WithValidCommand_ShouldDelegateToCommandDispatcher()
    {
        // Arrange
        var mediator = GetService<IMediator>();
        var command = new TestCommand("Test Command");

        // Act
        var result = await mediator.SendAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_WithValidCommandWithResult_ShouldDelegateToCommandDispatcher()
    {
        // Arrange
        var mediator = GetService<IMediator>();
        var command = new TestCommandWithResult("Test Command With Result");

        // Act
        var result = await mediator.SendAsync<TestCommandWithResult, TestResult>(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(command.Name);
    }

    [Fact]
    public async Task QueryAsync_WithValidQuery_ShouldDelegateToQueryDispatcher()
    {
        // Arrange
        var mediator = GetService<IMediator>();
        var queryId = Guid.NewGuid();
        var query = new TestQuery(queryId);

        // Act
        var result = await mediator.QueryAsync<TestQuery, TestResult>(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(queryId);
        result.Value.Name.Should().Be("Test Item");
    }

    [Fact]
    public async Task SendAsync_WithCancellationToken_ShouldPassTokenToDispatcher()
    {
        // Arrange
        var mediator = GetService<IMediator>();
        var command = new TestCommand("Test Command");
        using var cts = new CancellationTokenSource();

        // Act
        var result = await mediator.SendAsync(command, cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task QueryAsync_WithCancellationToken_ShouldPassTokenToDispatcher()
    {
        // Arrange
        var mediator = GetService<IMediator>();
        var query = new TestQuery(Guid.NewGuid());
        using var cts = new CancellationTokenSource();

        // Act
        var result = await mediator.QueryAsync<TestQuery, TestResult>(query, cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WhenCommandDispatcherIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Mediator(null!, Mock.Of<IQueryDispatcher>()));
    }

    [Fact]
    public void Constructor_WhenQueryDispatcherIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Mediator(Mock.Of<ICommandDispatcher>(), null!));
    }

    [Fact]
    public async Task Mediator_ShouldHandleComplexWorkflow()
    {
        // Arrange
        var mediator = GetService<IMediator>();
        var commandId = Guid.NewGuid();

        // Act - Execute a command that creates something
        var createCommand = new TestCommandWithResult($"Create Item {commandId}");
        var createResult = await mediator.SendAsync<TestCommandWithResult, TestResult>(createCommand);

        // Then query for what we created
        var query = new TestQuery(createResult.Value.Id);
        var queryResult = await mediator.QueryAsync<TestQuery, TestResult>(query);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        createResult.Value.Name.Should().Contain("Create Item");

        queryResult.IsSuccess.Should().BeTrue();
        queryResult.Value.Id.Should().Be(createResult.Value.Id);
    }
}
