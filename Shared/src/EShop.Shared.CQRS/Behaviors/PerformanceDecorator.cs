using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.CQRS.Query;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EShop.Shared.CQRS.Behaviors;

internal static class PerformanceDecorator
{
    internal sealed class QueryHandler<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        private readonly Stopwatch _timer;
        private readonly ILogger<QueryHandler<TQuery, TResponse>> _logger;
        private readonly IQueryHandler<TQuery, TResponse> _innerHandler;

        public QueryHandler(
            ILogger<QueryHandler<TQuery, TResponse>> logger,
            IQueryHandler<TQuery, TResponse> innerHandler)
        {
            _logger = logger;
            _timer = new Stopwatch();
            _innerHandler = innerHandler;
        }

        public async Task<Result<TResponse>> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
        {
            _timer.Start();
            var result = await _innerHandler.HandleAsync(query, cancellationToken);
            _timer.Stop();

            var elapsedMilliseconds = _timer.ElapsedMilliseconds;
            if (elapsedMilliseconds <= 5000)
            {
                return result;
            }

            var requestName = typeof(TQuery).Name;
            _logger.LogDebug("Long Time Running: Request Details: {Name} ({ElapsedMilliseconds} milliseconds)", requestName, elapsedMilliseconds);

            return result;
        }
    }

    internal sealed class CommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly Stopwatch _timer;
        private readonly ILogger<CommandHandler<TCommand>> _logger;
        private readonly ICommandHandler<TCommand> _innerHandler;

        public CommandHandler(
            ILogger<CommandHandler<TCommand>> logger,
            ICommandHandler<TCommand> innerHandler)
        {
            _logger = logger;
            _timer = new Stopwatch();
            _innerHandler = innerHandler;
        }

        public async Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            _timer.Start();
            var result = await _innerHandler.HandleAsync(command, cancellationToken);
            _timer.Stop();

            var elapsedMilliseconds = _timer.ElapsedMilliseconds;
            if (elapsedMilliseconds <= 5000)
            {
                return result;
            }

            var requestName = typeof(TCommand).Name;
            _logger.LogDebug("Long Time Running: Request Details: {Name} ({ElapsedMilliseconds} milliseconds)", requestName, elapsedMilliseconds);

            return result;
        }
    }

    internal sealed class CommandHandler<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        private readonly Stopwatch _timer;
        private readonly ILogger<CommandHandler<TCommand, TResponse>> _logger;
        private readonly ICommandHandler<TCommand, TResponse> _innerHandler;

        public CommandHandler(
            ILogger<CommandHandler<TCommand, TResponse>> logger,
            ICommandHandler<TCommand, TResponse> innerHandler)
        {
            _logger = logger;
            _timer = new Stopwatch();
            _innerHandler = innerHandler;
        }


        public async Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            _timer.Start();
            var result = await _innerHandler.HandleAsync(command, cancellationToken);
            _timer.Stop();

            var elapsedMilliseconds = _timer.ElapsedMilliseconds;
            if (elapsedMilliseconds <= 5000)
            {
                return result;
            }

            var requestName = typeof(TCommand).Name;
            _logger.LogDebug("Long Time Running: Request Details: {Name} ({ElapsedMilliseconds} milliseconds)", requestName, elapsedMilliseconds);

            return result;
        }
    }
}
