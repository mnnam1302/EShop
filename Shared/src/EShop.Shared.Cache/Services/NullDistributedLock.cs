using EShop.Shared.Scoping.ResourceAccessControl.Providers;

namespace EShop.Shared.Cache.Services;

/// <summary>
/// A no-op <see cref="IDistributedLock"/> used when distributed infrastructure (Redis) is not
/// available, e.g. in development or test environments that rely on the in-memory cache.
/// Because only a single process is running, there is no cross-instance contention, so every
/// acquire attempt succeeds immediately.
/// </summary>
internal sealed class NullDistributedLock : IDistributedLock
{
    public Task<IDisposable?> TryAcquireAsync(
        string resource,
        TimeSpan expirationTime,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IDisposable?>(NullLockHandle.Instance);

    private sealed class NullLockHandle : IDisposable
    {
        public static readonly NullLockHandle Instance = new();
        public void Dispose() { }
    }
}
