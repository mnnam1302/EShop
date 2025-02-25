using EShop.Tenancy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Tenancy.Persistence;

public class TenancyDbContext : DbContext
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
}