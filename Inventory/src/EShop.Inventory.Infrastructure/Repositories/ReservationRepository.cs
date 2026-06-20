using EShop.Inventory.Domain.Abstractions;
using EShop.Inventory.Domain.Aggregates;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Inventory.Infrastructure.Repositories;

public sealed class ReservationRepository : RepositoryBase<InventoryDbContext, Reservation, Guid>, IReservationRepository
{
    public ReservationRepository(InventoryDbContext dbContext) : base(dbContext)
    {
    }
}
