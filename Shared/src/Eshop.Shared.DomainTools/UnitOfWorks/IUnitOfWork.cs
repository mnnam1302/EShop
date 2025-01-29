namespace EShop.Shared.DomainTools.UnitOfWorks;

public interface IUnitOfWork : IAsyncDisposable
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}