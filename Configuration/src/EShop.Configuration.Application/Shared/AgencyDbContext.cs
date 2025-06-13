using EShop.Configuration.Application.Agencies;
using Microsoft.EntityFrameworkCore;

namespace EShop.Configuration.Application.Shared;

public class AgencyDbContext : DbContext
{
    public DbSet<Agency> Agencies { get; set; }

    public DbSet<SalesChannel> SaleChannels { get; set; }

    public AgencyDbContext(DbContextOptions<AgencyDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);
    }
}