using EShop.Finance.Domain.Abstractions;
using EShop.Finance.Domain.Aggregates.AccountingCompany;
using EShop.Shared.DomainTools.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EShop.Finance.Infrastructure.Repositories;

public sealed class AccountingCompanyRepository : RepositoryBase<FinanceDbContext, AccountingCompany, Guid>, IAccountingCompanyRepository
{
    private readonly FinanceDbContext _dbContext;

    public AccountingCompanyRepository(FinanceDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AccountingCompany?> FindByTenantIdAsync(string tenantId, bool trackChanges = false, CancellationToken cancellationToken = default)
    {
        var query = trackChanges ? _dbContext.AccountingCompanies : _dbContext.AccountingCompanies.AsNoTracking();
        return query.FirstOrDefaultAsync(c => c.TenantId == tenantId, cancellationToken);
    }
}
