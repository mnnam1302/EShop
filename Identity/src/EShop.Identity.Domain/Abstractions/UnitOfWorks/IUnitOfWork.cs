namespace EShop.Identity.Domain.Abstractions.UnitOfWorks;

public interface IUnitOfWork : IAsyncDisposable
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}