using EShop.Shared.Contracts.Abstractions.Shared;
using MediatR;

namespace EShop.Shared.Contracts.Abstractions.Requests;

[Obsolete]
public interface ICommand : IRequest<Result>
{
}

[Obsolete]
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}

[Obsolete]
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

[Obsolete]
public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
