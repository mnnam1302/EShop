using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Abstractions.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.CQRS.Command;

public sealed class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Result> DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        ArgumentNullException.ThrowIfNull(command);

        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        var result = await handler.HandleAsync(command, cancellationToken);

        return result;
    }

    public async Task<Result<TResult>> DispatchAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        ArgumentNullException.ThrowIfNull(command);

        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        var result = await handler.HandleAsync(command, cancellationToken);

        return result;
    }
}
