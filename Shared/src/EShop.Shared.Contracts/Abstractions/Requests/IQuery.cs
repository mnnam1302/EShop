using EShop.Shared.Contracts.Abstractions.Shared;
using MediatR;

namespace EShop.Shared.Contracts.Abstractions.Requests;

[Obsolete]
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

[Obsolete]
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
