using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.CQRS.Query;

namespace EShop.Shared.CQRS;

public interface IMediator
{
    Task<Result> SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;

    Task<Result<TResult>> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>;

    Task<Result<TResult>> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>;
}
