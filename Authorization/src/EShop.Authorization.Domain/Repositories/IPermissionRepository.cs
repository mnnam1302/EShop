using EShop.Authorization.Domain.Entities;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Authorization.Domain.Repositories;

public interface IPermissionRepository : IRepositoryBase<Permission, string>
{
}
