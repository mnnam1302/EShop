using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.DbResourceAccessControl.Extensions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace EShop.Shared.DbResourceAccessControl.Interceptors;

public interface IMultiTenantIsolationStrategy : IDbConnectionInterceptor;

internal sealed class PostgresMultiTenantConnectionInterceptor : DbConnectionInterceptor, IMultiTenantIsolationStrategy
{
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly ILogger<PostgresMultiTenantConnectionInterceptor> _logger;

    public PostgresMultiTenantConnectionInterceptor(
        IUserDetailsProvider userDetailsProvider,
        ILogger<PostgresMultiTenantConnectionInterceptor> logger)
    {
        _userDetailsProvider = userDetailsProvider;
        _logger = logger;
    }

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        base.ConnectionOpened(connection, eventData);
        if (TryCreateTenantContext(connection, out DbCommand? setContextCommand))
        {
            setContextCommand.ExecuteNonQuery();
            setContextCommand.Dispose();
        }
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
        if (TryCreateTenantContext(connection, out DbCommand? setContextCommand))
        {
            await setContextCommand.ExecuteNonQueryAsync(cancellationToken);
            await setContextCommand.DisposeAsync();
        }
    }

    private bool TryCreateTenantContext(DbConnection dbConnection, [NotNullWhen(true)] out DbCommand? setContextCommand)
    {
        if (!_userDetailsProvider.IsAuthenticatedUser)
        {
            // Any calls without user in http context would crash but might be needed in some cases (e.g. VS debugger).
            // It is not responsibility of this class to make sure only authenticated users will get access
            // but if the connection doesn't have 'app.tenant_id' set the query will not return results anyway.
            _logger.LogWarning("Opening db connection without tenant isolation. Scoped queries will not return any results! [{providerId}]", _userDetailsProvider.GetHashCode());
            setContextCommand = null;
            return false;
        }

        var tenantId = _userDetailsProvider.AuthenticatedUser.TenantId;
        if (!string.IsNullOrEmpty(tenantId))
        {
            // PostgreSQL SET statement cannot be parameterized.
            setContextCommand = dbConnection.CreateCommand()
                .WithPostgreSqlCommandText($"SET app.tenant_id = '{tenantId}';");

            _logger.LogTrace("Setting connection tenant context to '{id}'.", tenantId);
            return true;
        }

        _logger.LogTrace("Opening db connection for authenticated user with an EMPTY tenant context");
        setContextCommand = null;
        return false;
    }
}