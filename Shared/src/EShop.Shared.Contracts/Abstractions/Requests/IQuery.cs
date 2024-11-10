using EShop.Shared.Contracts.Abstractions.Shared;
using MediatR;

namespace EShop.Shared.Contracts.Abstractions.Requests;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}