using EShop.Catalog.Application.Agencies;
using EShop.Shared.EventBus;
using EShop.Shared.Sequences;
using Microsoft.EntityFrameworkCore;

namespace EShop.Catalog.Application.Shared;

public class CatalogDbContext : DbContext, IInboxDbContext, ISequenceDbContextStore
{
    public DbSet<Agency> Agencies { get; set; }
    public DbSet<InboxMessage> InboxMessages { get; set; }
    public DbSet<Sequence> Sequences { get; set; }

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);
    }
}
