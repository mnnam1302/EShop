using EShop.Shared.Contracts.Abstractions.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShop.Shared.CQRS.Query;

public sealed class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QueryDispatcher> _logger;

    public QueryDispatcher(IServiceProvider serviceProvider, ILogger<QueryDispatcher> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<TResult>> DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        ArgumentNullException.ThrowIfNull(query);

        var queryType = typeof(TQuery);
        _logger.LogDebug("Dispatching query {QueryType} with result {ResultType}", queryType.Name, typeof(TResult).Name);

        var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        var result = await handler.HandleAsync(query, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogDebug("Query {QueryType} handled successfully", queryType.Name);
        }
        else
        {
            _logger.LogWarning("Query {QueryType} failed: {Error}", queryType.Name, result.Error.Message);
        }

        return result;
    }
}