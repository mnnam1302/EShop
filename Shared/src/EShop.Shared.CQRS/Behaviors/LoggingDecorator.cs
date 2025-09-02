using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.CQRS.Query;
using Microsoft.Extensions.Logging;

namespace EShop.Shared.CQRS.Behaviors;

public static class LoggingDecorator
{
    internal sealed class QueryHandler<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        private readonly ILogger<QueryHandler<TQuery, TResponse>> _logger;
        private readonly IQueryHandler<TQuery, TResponse> _innerHandler;

        public QueryHandler(
            ILogger<QueryHandler<TQuery, TResponse>> logger,
            IQueryHandler<TQuery, TResponse> innerHandler)
        {
            _logger = logger;
            _innerHandler = innerHandler;
        }

        public async Task<Result<TResponse>> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
        {
            var requestName = typeof(TQuery).Name;

            _logger.LogInformation("Processing request {RequestName}", requestName);

            var result = await _innerHandler.HandleAsync(query, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Completed request {RequestName}", requestName);
            }
            else
            {
                _logger.LogWarning("Completed request  {RequestName} with error: {Error}", requestName, result.Error);
            }

            return result;
        }
    }

    internal sealed class CommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly ILogger<CommandHandler<TCommand>> _logger;
        private readonly ICommandHandler<TCommand> _innerHandler;

        public CommandHandler(
            ILogger<CommandHandler<TCommand>> logger,
            ICommandHandler<TCommand> innerHandler)
        {
            _logger = logger;
            _innerHandler = innerHandler;
        }

        public async Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            var requestName = typeof(TCommand).Name;

            _logger.LogInformation("Processing request {RequestName}", requestName);

            var result = await _innerHandler.HandleAsync(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Completed request {RequestName}", requestName);
            }
            else
            {
                _logger.LogWarning("Completed request  {RequestName} with error: {Error}", requestName, result.Error);
            }

            return result;
        }
    }

    internal sealed class CommandHandler<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        private readonly ILogger<CommandHandler<TCommand, TResponse>> _logger;
        private readonly ICommandHandler<TCommand, TResponse> _innerHandler;

        public CommandHandler(
            ILogger<CommandHandler<TCommand, TResponse>> logger,
            ICommandHandler<TCommand, TResponse> innerHandler)
        {
            _logger = logger;
            _innerHandler = innerHandler;
        }

        public async Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            var requestName = typeof(TCommand).Name;

            _logger.LogInformation("Processing request {RequestName}", requestName);

            var result = await _innerHandler.HandleAsync(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Completed request {RequestName}", requestName);
            }
            else
            {
                _logger.LogWarning("Completed request  {RequestName} with error: {Error}", requestName, result.Error);
            }

            return result;
        }
    }
}
