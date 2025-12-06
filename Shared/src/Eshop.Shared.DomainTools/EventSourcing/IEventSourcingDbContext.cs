using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace EShop.Shared.DomainTools.EventSourcing;

public interface IEventStoreDbContext
{
    DbSet<EventStore> EventStores { get; }
}

public interface ISnapshotDbContext
{
    DbSet<Snapshot> Snapshots { get; }
}

public interface IEventSourcingDbContext : IEventStoreDbContext, ISnapshotDbContext
{
}