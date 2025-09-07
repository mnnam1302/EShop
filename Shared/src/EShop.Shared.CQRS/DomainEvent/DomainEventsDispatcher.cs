using EShop.Shared.Contracts.Abstractions.MessageBus;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EShop.Shared.CQRS.DomainEvent;

internal sealed class DomainEventsDispatcher : IDomainEventsDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventsDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();

            Type handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            IEnumerable<object?> handlers = scope.ServiceProvider.GetServices(handlerType);

            foreach (object? handler in handlers)
            {
                if (handler is null)
                {
                    continue;
                }

                MethodInfo? methodInfo = handlerType.GetMethod("Handle");
                if (methodInfo is not null)
                {
                    await (Task)methodInfo.Invoke(handler, [domainEvents, cancellationToken])!;
                }
            }
        }
    }
}
