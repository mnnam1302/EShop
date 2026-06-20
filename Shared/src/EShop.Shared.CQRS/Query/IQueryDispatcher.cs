using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Abstractions.Shared;

namespace EShop.Shared.CQRS.Query;

public interface IQueryDispatcher
{
    Task<Result<TResult>> DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>;
}
