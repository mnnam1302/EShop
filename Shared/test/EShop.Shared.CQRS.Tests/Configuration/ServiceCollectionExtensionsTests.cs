using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.CQRS.Tests.Configuration;

public class ServiceCollectionExtensionsTests : TestBase
{
    [Fact]
    public void AddCQRS_ShouldRegisterAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddCQRS();

        // Assert
        using var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetService<ICommandDispatcher>().Should().NotBeNull();
        serviceProvider.GetService<IQueryDispatcher>().Should().NotBeNull();
        serviceProvider.GetService<IMediator>().Should().NotBeNull();

        serviceProvider.GetService<CommandDispatcher>().Should().NotBeNull();
        serviceProvider.GetService<QueryDispatcher>().Should().NotBeNull();
        serviceProvider.GetService<Mediator>().Should().NotBeNull();
    }

    [Fact]
    public void AddCQRS_ShouldRegisterServicesWithCorrectLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddCQRS();

        // Assert
        using var serviceProvider = services.BuildServiceProvider();

        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();

        var commandDispatcher1 = scope1.ServiceProvider.GetService<ICommandDispatcher>();
        var commandDispatcher2 = scope1.ServiceProvider.GetService<ICommandDispatcher>();
        var commandDispatcher3 = scope2.ServiceProvider.GetService<ICommandDispatcher>();

        commandDispatcher1.Should().BeSameAs(commandDispatcher2);
        commandDispatcher1.Should().NotBeSameAs(commandDispatcher3);

        var mediator1 = scope1.ServiceProvider.GetService<IMediator>();
        var mediator2 = scope1.ServiceProvider.GetService<IMediator>();

        mediator1.Should().NotBeSameAs(mediator2);
    }

    [Fact]
    public void AddCQRS_WithCustomHandlers_ShouldResolveHandlersCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCQRS();

        services.AddScoped<ICommandHandler<TestCommand>, TestCommandHandler>();
        services.AddScoped<IQueryHandler<TestQuery, TestResult>, TestQueryHandler>();

        // Act
        using var serviceProvider = services.BuildServiceProvider();

        // Assert
        var commandHandler = serviceProvider.GetService<ICommandHandler<TestCommand>>();
        var queryHandler = serviceProvider.GetService<IQueryHandler<TestQuery, TestResult>>();

        commandHandler.Should().NotBeNull();
        commandHandler.Should().BeOfType<TestCommandHandler>();

        queryHandler.Should().NotBeNull();
        queryHandler.Should().BeOfType<TestQueryHandler>();
    }

    [Fact]
    public async Task AddCQRS_IntegrationTest_ShouldWorkEndToEnd()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCQRS();
        services.AddScoped<ICommandHandler<TestCommand>, TestCommandHandler>();
        services.AddScoped<ICommandHandler<TestCommandWithResult, TestResult>, TestCommandWithResultHandler>();
        services.AddScoped<IQueryHandler<TestQuery, TestResult>, TestQueryHandler>();

        using var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act & Assert
        var command = new TestCommand("Integration Test Command");
        var commandResult = await mediator.SendAsync(command);
        commandResult.IsSuccess.Should().BeTrue();

        // Act & Assert
        var commandWithResult = new TestCommandWithResult("Integration Test Command With Result");
        var commandWithResultResult = await mediator.SendAsync<TestCommandWithResult, TestResult>(commandWithResult);
        commandWithResultResult.IsSuccess.Should().BeTrue();
        commandWithResultResult.Value.Should().NotBeNull();

        // Act & Assert
        var query = new TestQuery(Guid.NewGuid());
        var queryResult = await mediator.QueryAsync<TestQuery, TestResult>(query);
        queryResult.IsSuccess.Should().BeTrue();
        queryResult.Value.Should().NotBeNull();
    }

    [Fact]
    public void AddCQRS_MultipleCalls_ShouldNotDuplicateRegistrations()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddCQRS();
        services.AddCQRS();

        // Assert
        using var serviceProvider = services.BuildServiceProvider();

        var mediator = serviceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        var mediatorDescriptors = services.Where(s => s.ServiceType == typeof(IMediator)).ToList();
        mediatorDescriptors.Should().HaveCount(2, "One interface registration and one concrete registration");
    }

    [Fact]
    public void AddCQRS_WithoutLogging_ShouldStillWork()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS();

        // Assert
        using var serviceProvider = services.BuildServiceProvider();

        var mediator = serviceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        var commandDispatcher = serviceProvider.GetService<ICommandDispatcher>();
        var queryDispatcher = serviceProvider.GetService<IQueryDispatcher>();

        commandDispatcher.Should().NotBeNull();
        queryDispatcher.Should().NotBeNull();
    }
}
