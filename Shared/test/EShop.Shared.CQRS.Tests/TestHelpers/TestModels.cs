using EShop.Shared.CQRS.Command;
using EShop.Shared.CQRS.Query;
using EShop.Shared.Contracts.Abstractions.Shared;

namespace EShop.Shared.CQRS.Tests.TestHelpers;

// Test Commands
public record TestCommand(string Name) : ICommand;

public record TestCommandWithResult(string Name) : ICommand<TestResult>;

public record TestResult(Guid Id, string Name, DateTime CreatedAt);

// Test Queries
public record TestQuery(Guid Id) : IQuery<TestResult>;

public record TestQueryWithMultipleResults(string SearchTerm) : IQuery<IEnumerable<TestResult>>;

// Test Command Handlers
public class TestCommandHandler : ICommandHandler<TestCommand>
{
    public bool WasCalled { get; private set; }
    public TestCommand? LastCommand { get; private set; }

    public Task<Result> HandleAsync(TestCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        WasCalled = true;
        LastCommand = command;
        return Task.FromResult(Result.Success());
    }
}

public class TestCommandWithResultHandler : ICommandHandler<TestCommandWithResult, TestResult>
{
    public bool WasCalled { get; private set; }
    public TestCommandWithResult? LastCommand { get; private set; }

    public Task<Result<TestResult>> HandleAsync(TestCommandWithResult command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        WasCalled = true;
        LastCommand = command;
        
        var result = new TestResult(Guid.NewGuid(), command.Name, DateTime.UtcNow);
        return Task.FromResult(Result.Success(result));
    }
}

public class FailingCommandHandler : ICommandHandler<TestCommand>
{
    public Task<Result> HandleAsync(TestCommand command, CancellationToken cancellationToken)
    {
        var error = new Error("Test.Failed", "This command handler always fails");
        return Task.FromResult(Result.Failure(error));
    }
}

public class ThrowingCommandHandler : ICommandHandler<TestCommand>
{
    public Task<Result> HandleAsync(TestCommand command, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Test exception");
    }
}

// Test Query Handlers
public class TestQueryHandler : IQueryHandler<TestQuery, TestResult>
{
    public bool WasCalled { get; private set; }
    public TestQuery? LastQuery { get; private set; }

    public Task<Result<TestResult>> HandleAsync(TestQuery query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        WasCalled = true;
        LastQuery = query;
        
        var result = new TestResult(query.Id, "Test Item", DateTime.UtcNow);
        return Task.FromResult(Result.Success(result));
    }
}

public class FailingQueryHandler : IQueryHandler<TestQuery, TestResult>
{
    public Task<Result<TestResult>> HandleAsync(TestQuery query, CancellationToken cancellationToken)
    {
        var error = new Error("Query.Failed", "This query handler always fails");
        return Task.FromResult(Result.Failure<TestResult>(error));
    }
}

public class ThrowingQueryHandler : IQueryHandler<TestQuery, TestResult>
{
    public Task<Result<TestResult>> HandleAsync(TestQuery query, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Test query exception");
    }
}
