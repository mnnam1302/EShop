using EShop.Inventory.Domain.Abstractions;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Inventory.Infrastructure.Repositories;

internal sealed class InventoryRepository : RepositoryBase<InventoryDbContext, Domain.Entities.Inventory, Guid>, IInventoryRepository
{
    public InventoryRepository(InventoryDbContext dbContext) : base(dbContext)
    {
    }
}
