using EShop.Inventory.Domain.Aggregates;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Inventory.Domain.Abstractions;

public interface IReservationRepository : IRepositoryBase<Reservation, Guid>
{
}
