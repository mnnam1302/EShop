using EShop.Inventory.Domain.Aggregates;
using EShop.Shared.EventBus;
using EShop.Shared.EventBus.DependencyInjections.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EShop.Inventory.Infrastructure;

public class InventoryDbContext : DbContext, IInboxDbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<InboxMessage> InboxMessages { get; set; }
    public DbSet<Domain.Aggregates.Inventory> Inventories { get; set; }
    public DbSet<Reservation> Reservations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);
        modelBuilder.AddInboxMessageEntity();
    }
}
