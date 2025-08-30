using EShop.Shared.Contracts.Abstractions.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.CQRS.Query;

public sealed class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public QueryDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Result<TResult>> DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken)
        where TQuery : IQuery<TResult>
    {
        ArgumentNullException.ThrowIfNull(query);

        var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        var result = await handler.HandleAsync(query, cancellationToken);

        return result;
    }
}