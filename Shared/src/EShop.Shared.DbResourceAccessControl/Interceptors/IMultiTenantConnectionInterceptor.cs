using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EShop.Shared.DbResourceAccessControl.Interceptors;

public interface IMultiTenantIsolationStrategy : IDbConnectionInterceptor;