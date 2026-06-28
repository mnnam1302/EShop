using EShop.Finance.Domain.Aggregates.Account;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Finance.Domain.Abstractions;

public interface IAccountRepository : IRepositoryBase<Account, Guid>
{
    Task<Account?> FindByOrderIdAsync(Guid orderId, bool trackChanges = false, CancellationToken cancellationToken = default);

    Task<Account?> FindByPaymentIdAsync(Guid paymentId, bool trackChanges = false, CancellationToken cancellationToken = default);
}
