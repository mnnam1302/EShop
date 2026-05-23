using EShop.Order.Domain.Repositories;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Order.Infrastructure.Repositories;

internal sealed class OrderRepository : RepositoryBase<OrderDbContext, Domain.Aggregates.Order, Guid>, IOrderRepository
{
    public OrderRepository(OrderDbContext dbContext) : base(dbContext)
    {
    }
}
