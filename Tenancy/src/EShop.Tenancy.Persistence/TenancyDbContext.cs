using EShop.Shared.EventBus;
using EShop.Tenancy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Tenancy.Persistence;

public class TenancyDbContext : DbContext, IInboxDbContext
{
    public TenancyDbContext(DbContextOptions<TenancyDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenancyDbContext).Assembly);
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<TenantFeature> TenantFeatures { get; set; }
    public DbSet<TenantSetting> TenantSettings { get; set; }
    public DbSet<InboxMessage> InboxMessages { get; set; }
}