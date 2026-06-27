using EShop.Finance.Domain.Abstractions;
using EShop.Finance.Domain.Aggregates.Account;
using EShop.Shared.DomainTools.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EShop.Finance.Infrastructure.Repositories;

public sealed class AccountRepository : RepositoryBase<FinanceDbContext, Account, Guid>, IAccountRepository
{
    private readonly FinanceDbContext _dbContext;

    public AccountRepository(FinanceDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Account?> FindByOrderIdAsync(Guid orderId, bool trackChanges = false, CancellationToken cancellationToken = default)
    {
        var query = trackChanges ? _dbContext.Accounts : _dbContext.Accounts.AsNoTracking();
        return query
            .Include(a => a.Payments)
            .FirstOrDefaultAsync(a => a.OrderId == orderId, cancellationToken);
    }

    public Task<Account?> FindByPaymentIdAsync(Guid paymentId, bool trackChanges = false, CancellationToken cancellationToken = default)
    {
        var query = trackChanges ? _dbContext.Accounts : _dbContext.Accounts.AsNoTracking();
        return query
            .Include(a => a.Payments)
            .FirstOrDefaultAsync(a => a.Payments.Any(i => i.Id == paymentId), cancellationToken);
    }
}
