using EShop.Shared.Contracts.Abstractions.Shared;

namespace EShop.Shared.CQRS.Command;

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

public abstract class CommandHandler<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public abstract Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

public abstract class CommandHandler<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public abstract Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
