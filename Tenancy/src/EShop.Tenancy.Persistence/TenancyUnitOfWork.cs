using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Tenancy.Domain;

namespace EShop.Tenancy.Persistence;

public class TenancyUnitOfWork : EFUnitOfWork<TenancyDbContext>, ITenancyUnitOfWork
{
    public TenancyUnitOfWork(TenancyDbContext dbContext) : base(dbContext)
    {
    }
}