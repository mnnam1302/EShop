namespace EShop.Shared.DomainTools.EventSourcing.SeedWork;

public sealed class Snapshot
{
    public Guid Id { get; set; }
    public Guid AggregateId { get; set; }
    public string AggregateType { get; set; } = string.Empty;
    public IAggregate Aggregate { get; set; }
    public ulong Version { get; set; }
    public DateTimeOffset TimeStampUtc { get; set; }

    public static Snapshot Create(IAggregate aggregate, EventStore eventStore)
    {
        var snapshot = new Snapshot
        {
            Id = Guid.NewGuid(),
            AggregateId = aggregate.Id,
            AggregateType = aggregate.GetType().Name,
            Aggregate = aggregate,
            Version = eventStore.Version,
            TimeStampUtc = eventStore.TimeStampUtc
        };

        return snapshot;
    }
}