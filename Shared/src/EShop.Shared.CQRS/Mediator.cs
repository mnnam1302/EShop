using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.CQRS.Query;

namespace EShop.Shared.CQRS;

public sealed class Mediator : IMediator
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;

    public Mediator(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
    }

    public async Task<Result> SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        return await _commandDispatcher.DispatchAsync(command, cancellationToken);
    }

    public async Task<Result<TResult>> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        return await _commandDispatcher.DispatchAsync<TCommand, TResult>(command, cancellationToken);
    }

    public async Task<Result<TResult>> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        return await _queryDispatcher.DispatchAsync<TQuery, TResult>(query, cancellationToken);
    }
}
