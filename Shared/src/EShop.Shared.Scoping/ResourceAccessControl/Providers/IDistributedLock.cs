namespace EShop.Shared.Scoping.ResourceAccessControl.Providers;

public interface IDistributedLock
{
    Task<IDisposable?> TryAcquireAsync(
        string resource,
        TimeSpan expirationTime,
        CancellationToken cancellationToken = default);
}
