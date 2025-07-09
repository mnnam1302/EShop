using EShop.Configuration.Application.Agencies;
using EShop.Shared.EventBus;
using EShop.Shared.Sequences;
using Microsoft.EntityFrameworkCore;

namespace EShop.Configuration.Application.Shared;

public class ConfigurationDbContext : DbContext, IInboxDbContext, ISequenceDbContextStore
{
    public DbSet<Agency> Agencies { get; set; }
    public DbSet<SalesChannel> SaleChannels { get; set; }
    public DbSet<InboxMessage> InboxMessages { get; set; }
    public DbSet<Sequence> Sequences { get; set; }

    public ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);
    }
}