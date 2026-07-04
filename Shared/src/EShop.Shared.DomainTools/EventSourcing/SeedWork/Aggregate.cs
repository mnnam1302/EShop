using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.DomainTools.Entities;
using System.Text.Json.Serialization;

namespace EShop.Shared.DomainTools.EventSourcing.SeedWork;

public interface IAggregate : IEntityBase<Guid>
{
    IEnumerable<IDomainEvent> UncommittedEvents { get; }

    void Replay(IEnumerable<IDomainEvent> events);

    void MarkEventsAsCommitted();
}

public abstract class Aggregate : IAggregate
{
    private readonly List<IDomainEvent> _uncommittedEvents = [];

    public Guid Id { get; set; }
    public ulong Version { get; private set; }

    public virtual bool IsNew => Version <= 0;

    [JsonIgnore]
    public IEnumerable<IDomainEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();

    public void MarkEventsAsCommitted()
    {
        _uncommittedEvents.Clear();
    }

    public void Replay(IEnumerable<IDomainEvent> events)
    {
        foreach (var @event in events)
        {
            Apply(@event);
            Version = @event.Version;
        }
    }

    protected void RaiseEvent(IDomainEvent @event)
    {
        Version++;
        @event.Version = Version;
        Apply(@event);
        _uncommittedEvents.Add(@event);
    }

    protected void Apply(IDomainEvent @event)
    {
        var method = this.GetType().GetMethod(
            "Apply",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
            [@event.GetType()]);
        if (method == null)
        {
            throw new InvalidOperationException($"Method Apply for {@event.GetType()} not found");
        }
        else
        {
            method.Invoke(this, [@event]);
        }
    }
}
