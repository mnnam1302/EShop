using EShop.Shared.Contracts.Abstractions.Shared;

namespace EShop.Shared.CQRS.Command;

public interface ICommandDispatcher
{
    Task<Result> DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;

    Task<Result<TResult>> DispatchAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>;
}
