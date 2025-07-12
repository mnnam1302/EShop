using EShop.Shared.Contracts.Abstractions.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShop.Shared.CQRS.Command;

public sealed class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandDispatcher> _logger;

    public CommandDispatcher(IServiceProvider serviceProvider, ILogger<CommandDispatcher> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        ArgumentNullException.ThrowIfNull(command);

        var commandType = typeof(TCommand);
        _logger.LogDebug("Dispatching command {CommandType}", commandType.Name);

        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        var result = await handler.HandleAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogDebug("Command {CommandType} handled successfully", commandType.Name);
        }
        else
        {
            _logger.LogWarning("Command {CommandType} failed: {Error}", commandType.Name, result.Error.Message);
        }

        return result;
    }

    public async Task<Result<TResult>> DispatchAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        ArgumentNullException.ThrowIfNull(command);

        var commandType = typeof(TCommand);
        _logger.LogDebug("Dispatching command {CommandType} with result {ResultType}", commandType.Name, typeof(TResult).Name);

        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        var result = await handler.HandleAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogDebug("Command {CommandType} handled successfully", commandType.Name);
        }
        else
        {
            _logger.LogWarning("Command {CommandType} failed: {Error}", commandType.Name, result.Error.Message);
        }

        return result;
    }
}
