using EShop.Order.Application.Sagas;
using EShop.Shared.DomainTools.EventSourcing;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.JsonApi.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EShop.Order.Infrastructure;

public sealed class OrderDbContext : DbContext, IEventStoreDbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Aggregates.Order> Orders { get; set; }
    public DbSet<Domain.Aggregates.OrderItem> OrderItems { get; set; }

    public DbSet<EventStore> EventStores { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);
        modelBuilder.AddEventStoreEntity();
    }
}
