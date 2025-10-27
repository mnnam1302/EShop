using EShop.Shared.CQRS.DomainEvent;
using EShop.Shared.DomainTools.Aggregates;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EShop.Shared.DbResourceAccessControl.Interceptors;

public interface IDispatchDomainEventsInterceptor : ISaveChangesInterceptor
{
}

internal sealed class DispatchDomainEventsInterceptor : SaveChangesInterceptor, IDispatchDomainEventsInterceptor
{
    private readonly IDomainEventsDispatcher domainEventsDispatcher;

    public DispatchDomainEventsInterceptor(IDomainEventsDispatcher domainEventsDispatcher)
    {
        this.domainEventsDispatcher = domainEventsDispatcher;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;
        if (dbContext == null)
        {
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        var domainEvents = dbContext.ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(x => x.Entity)
            .SelectMany(aggregateRoot =>
            {
                return aggregateRoot.GetDomainEvents();
            });

        await domainEventsDispatcher.DispatchAsync(domainEvents, cancellationToken);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}
