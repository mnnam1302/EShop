using EShop.Shared.Contracts.Abstractions.Shared;

namespace EShop.Shared.CQRS.Query;

public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
