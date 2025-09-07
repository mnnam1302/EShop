using Microsoft.EntityFrameworkCore;

namespace EShop.Shared.DbResourceAccessControl;

public interface IRingFencingIsolationStrategy
{
    void AddRingFencingIsolation(DbContext dbContext);
}
