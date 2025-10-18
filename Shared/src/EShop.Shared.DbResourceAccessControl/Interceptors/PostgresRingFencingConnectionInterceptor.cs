using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace EShop.Shared.DbResourceAccessControl.Interceptors;

public interface IRingFencingConnectionInterceptor : IDbConnectionInterceptor;

public sealed class PostgresRingFencingConnectionInterceptor : DbConnectionInterceptor, IRingFencingConnectionInterceptor
{
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly IUserOrganizationContextProvider _userOrganizationContextProvider;
    private readonly ILogger<PostgresRingFencingConnectionInterceptor> _logger;

    public PostgresRingFencingConnectionInterceptor(
        IUserDetailsProvider userDetailsProvider,
        IUserOrganizationContextProvider userOrganizationContextProvider,
        ILogger<PostgresRingFencingConnectionInterceptor> logger)
    {
        _userDetailsProvider = userDetailsProvider;
        _userOrganizationContextProvider = userOrganizationContextProvider;
        _logger = logger;
    }

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        base.ConnectionOpened(connection, eventData);
        if (TryCreateRingFencingContext(connection, out DbCommand? setContextCommand))
        {
            setContextCommand.ExecuteNonQuery();
            setContextCommand.Dispose();
        }
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
        if (TryCreateRingFencingContext(connection, out DbCommand? setContextCommand))
        {
            await setContextCommand.ExecuteNonQueryAsync(cancellationToken);
            await setContextCommand.DisposeAsync();
        }
    }

    private bool TryCreateRingFencingContext(DbConnection connection, [NotNullWhen(true)] out DbCommand? setContextCommand)
    {
        if (!_userDetailsProvider.IsAuthenticatedUser)
        {
            // Any calls without user in http context would crash but might be needed in some cases (e.g. VS debugger)
            // It is not responsibility of this class to make sure only authenticated users will get access
            // but if the connection doesn't have 'app.scope' set the query will not return results anyway.
            _logger.LogWarning("Opening db connection without ring fencing. Scoped queries will not return any results! [{ProviderId}]", _userDetailsProvider.GetHashCode());
            setContextCommand = null;
            return false;
        }

        string? contextPath;
        if (_userDetailsProvider.IsSystemUser)
        {
            contextPath = _userDetailsProvider.AuthenticatedUser.TenantId;
        }
        else
        {
            var userOrganizationContext = GetUserOrganizationContext();
            contextPath = userOrganizationContext == null ? string.Empty : userOrganizationContext.OrganizationContextPath;
        }

        setContextCommand = connection.CreateCommand();
        setContextCommand.CommandText = string.IsNullOrWhiteSpace(contextPath)
            ? "SET app.scope = '';"
            : $"SET app.scope = '{contextPath}%';";

        _logger.LogTrace("Setting connection user context to '{Path}' for user '{User}'", contextPath, _userDetailsProvider.AuthenticatedUser.Id);
        return true;
    }

    private UserOrganizationContext? GetUserOrganizationContext()
    {
        UserOrganizationContext? userOrganizationContext;
        try
        {
            userOrganizationContext = AsyncContext.Run(
                () => _userOrganizationContextProvider.GetUserOrganizationContextForSpecificUserAsync(
                    _userDetailsProvider.AuthenticatedUser.Id,
                    _userDetailsProvider.AuthenticatedUser.UserType));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error retriving user organization context");
            userOrganizationContext = null;
        }

        return userOrganizationContext;
    }
}
