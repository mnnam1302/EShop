using EShop.Authorization.Application.Shared;
using EShop.Authorization.Domain.Entities;
using EShop.Authorization.Domain.Repositories;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Authorization.Application.Repositories;

public sealed class UserRepository : AggregateRepository<AuthorizationDbContext, User, string>, IUserRepository
{
    public UserRepository(AuthorizationDbContext dbContext) : base(dbContext)
    {
    }
}
