using EShop.Authorization.Domain.Entities;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Authorization.Domain.Repositories;

public interface IRoleRepository : IRepositoryBase<Role, Guid>
{
}
