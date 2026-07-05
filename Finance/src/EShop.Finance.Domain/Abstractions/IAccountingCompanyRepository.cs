using EShop.Finance.Domain.Aggregates.AccountingCompany;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Finance.Domain.Abstractions;

public interface IAccountingCompanyRepository : IRepositoryBase<AccountingCompany, Guid>
{
    Task<AccountingCompany?> FindByTenantIdAsync(string tenantId, bool trackChanges = false, CancellationToken cancellationToken = default);
}
