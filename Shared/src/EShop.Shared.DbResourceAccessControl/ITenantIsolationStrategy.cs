using Microsoft.EntityFrameworkCore;

namespace EShop.Shared.DbResourceAccessControl;

public interface ITenantIsolationStrategy
{
    void AddTenantIsolation(DbContext dbContext);
}