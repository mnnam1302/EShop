using EShop.Shared.Contracts.Abstractions.Shared;

namespace EShop.Shared.DomainTools.Exceptions;

public sealed class CommandException : Exception
{
    public Type CommandType { get; }
    public Result ExecutionResult { get; }

    public CommandException(Type commandType, Result executionResult, string message)
        : base(message)
    {
        CommandType = commandType;
        ExecutionResult = executionResult;
    }

    public CommandException(Type commandType, string message, Exception exception)
        : base(message, exception)
    {
        CommandType = commandType;

    }
}
