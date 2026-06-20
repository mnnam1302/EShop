using EShop.Shared.DomainTools.Repositories;

namespace EShop.Order.Domain.Repositories;

public interface IOrderRepository : IRepositoryBase<Aggregates.Order, Guid>
{
}
