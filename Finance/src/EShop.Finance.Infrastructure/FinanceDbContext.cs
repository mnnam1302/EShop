using EShop.Finance.Domain.Aggregates.Account;
using EShop.Finance.Domain.Aggregates.AccountingCompany;
using EShop.Finance.Infrastructure.Integration;
using EShop.Shared.EventBus;
using EShop.Shared.EventBus.DependencyInjections.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EShop.Finance.Infrastructure;

public sealed class FinanceDbContext : DbContext, IInboxDbContext
{
    public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options)
    {
    }

    public DbSet<InboxMessage> InboxMessages { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<AccountingCompany> AccountingCompanies { get; set; }
    public DbSet<IntegrationProviderSession> IntegrationProviderSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);
        modelBuilder.AddInboxMessageEntity();
    }
}
