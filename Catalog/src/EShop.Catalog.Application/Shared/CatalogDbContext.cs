using EShop.Catalog.Application.Agencies;
using EShop.Shared.DomainTools.EventSourcing;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.EventBus;
using EShop.Shared.EventBus.DependencyInjections.Extensions;
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);

        modelBuilder.AddEventStoreEntity();
        modelBuilder.AddInboxMessageEntity();
    }
}