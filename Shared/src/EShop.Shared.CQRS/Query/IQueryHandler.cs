using EShop.Shared.Contracts.Abstractions.Shared;

namespace EShop.Shared.CQRS.Query;

public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken);
}

public abstract class QueryHandler<TQuery, TResult> : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    public abstract Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
