using EShop.Shared.DomainTools.Repositories;

namespace EShop.Inventory.Domain.Abstractions;

public interface IInventoryRepository : IRepositoryBase<Entities.Inventory, Guid>
{
}
