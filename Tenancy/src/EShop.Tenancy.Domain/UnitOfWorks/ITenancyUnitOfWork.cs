using EShop.Shared.DomainTools.UnitOfWorks;

namespace EShop.Tenancy.Domain.UnitOfWorks;

public interface ITenancyUnitOfWork : IUnitOfWork
{
    /// <summary>
    /// Explicitly sets the PostgreSQL session variable <c>app.tenant_id</c> on the current
    /// connection. Required when performing cross-tenant writes inside an already-open
    /// transaction, where the connection interceptor cannot re-fire.
    /// </summary>
    Task SetTenantContextAsync(string tenantId, CancellationToken cancellationToken = default);
}
