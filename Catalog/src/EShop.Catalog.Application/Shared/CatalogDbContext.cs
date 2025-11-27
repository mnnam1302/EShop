using EShop.Catalog.Application.Agencies;
using EShop.Shared.DomainTools.EventSourcing;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.EventBus;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.Sequences;
using Microsoft.EntityFrameworkCore;

namespace EShop.Catalog.Application.Shared;

public sealed class CatalogDbContext : DbContext, IInboxDbContext, ISequenceDbContextStore, IEventStoreDbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Agency> Agencies { get; set; }
    public DbSet<InboxMessage> InboxMessages { get; set; }
    public DbSet<Sequence> Sequences { get; set; }
    public DbSet<EventStore> EventStores { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply configurations from current assembly (Catalog)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);

        // Apply configurations from EventBus assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InboxMessage).Assembly);

        // Apply configurations from DomainTools assembly (EventSourcing)
        modelBuilder.AddEventStoreEntity();
    }
}