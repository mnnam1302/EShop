using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EShop.Shared.DbResourceAccessControl;

public interface IMultiTenantIsolationStategy : IDbConnectionInterceptor
{
}