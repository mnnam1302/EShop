using EShop.Shared.Contract.Abstractions.Messages;
using EShop.Shared.Contract.Abstractions.Requests;
using Identity.Domain.Abstractions.Entities;

namespace Identity.Domain.Abstractions.Aggregates;

public abstract class AggregateRoot : EntityBase<Guid>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public long Version { get; private set; }

    public IEnumerable<IDomainEvent> GetDomainEvents() => _domainEvents;

    public abstract void Handle(ICommand command);

    //public void LoadFromHistory(IEnumerable<IDomainEvent> events)
    //{
    //    foreach (var @event in events)
    //    {
    //        Apply(@event);
    //        Version = @event.Version;
    //    }
    //}

    protected void RaiseEvent<TEvent>(Func<long, TEvent> func) where TEvent : IDomainEvent
        => RaiseEvent((func as Func<long, IDomainEvent>)!);

    protected void RaiseEvent(Func<long, IDomainEvent> onRaise)
    {
        Version++;
        var @event = onRaise(Version);
        Apply(@event);
        _domainEvents.Add(@event);
    }

    protected abstract void Apply(IDomainEvent domainEvent);
}