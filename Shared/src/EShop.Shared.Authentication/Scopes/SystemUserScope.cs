using EShop.Shared.Authentication.Abstractions;

namespace EShop.Shared.Authentication.Scopes;

/// <summary>
/// IDisposable scope that sets system user context on creation and clears it on disposal.
/// Use in background jobs, DB initializers, and service-internal scope changes.
/// </summary>
public sealed class SystemUserScope : IDisposable
{
    private readonly IUserDetailsProvider _userDetailsProvider;
    private bool _disposed;

    public SystemUserScope(IUserDetailsProvider userDetailsProvider, string? tenantId, string? userId = null, string? userType = null)
    {
        _userDetailsProvider = userDetailsProvider;

        if (tenantId is null)
        {
            _userDetailsProvider.SetSystemUserContextWithEmptyScope();
        }
        else
        {
            _userDetailsProvider.SetSystemUserContext(tenantId, userId, userType);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _userDetailsProvider.ClearSystemUserContext();
    }
}