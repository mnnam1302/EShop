using EShop.Shared.Contract.Abstractions.Shared;
using MediatR;

namespace EShop.Shared.Contract.Abstractions.Requests;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

public interface  IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}