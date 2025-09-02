using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.CQRS.Query;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EShop.Shared.CQRS.Behaviors;

public static class RequestTraicingBehavior
{
    public sealed class QueryHandler<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
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

            var requestName = typeof(TQuery).Name;
            var elapsedMilliseconds = _timer.ElapsedMilliseconds;
            _logger.LogInformation("Request Details: {RequestName} ({ElapsedMilliseconds} milliseconds)", requestName, elapsedMilliseconds);

            return result;
        }
    }

    public sealed class CommandHandler<TCommand> : ICommandHandler<TCommand>
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

            var requestName = typeof(TCommand).Name;
            var elapsedMilliseconds = _timer.ElapsedMilliseconds;
            _logger.LogInformation("Request Details: {RequestName} ({ElapsedMilliseconds} milliseconds)", requestName, elapsedMilliseconds);

            return result;
        }
    }

    public sealed class CommandHandler<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
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

            var requestName = typeof(TCommand).Name;
            var elapsedMilliseconds = _timer.ElapsedMilliseconds;
            _logger.LogInformation("Request Details: {RequestName} ({ElapsedMilliseconds} milliseconds)", requestName, elapsedMilliseconds);

            return result;
        }
    }
}
