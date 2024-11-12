using Serilog.Core;
using Serilog.Events;

namespace EShop.Shared.Scoping;

public class TenantUserEnricher : ILogEventEnricher
{
    public const string UserPropertyName = "User";
    public const string TenantPropertyName = "Tenant";

    private readonly IUserDetailsProvider? userDetailsProvider;

    public TenantUserEnricher() : this(null)
    {
    }

    public TenantUserEnricher(IUserDetailsProvider? userDetailsProvider)
    {
        this.userDetailsProvider = userDetailsProvider;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        try
        {
            if (this.userDetailsProvider != null && this.userDetailsProvider.IsAuthenticatedUser)
            {
                var user = userDetailsProvider.AuthenticatedUser;
                var userProperty = propertyFactory.CreateProperty(UserPropertyName, user.Username);
                logEvent.AddOrUpdateProperty(userProperty);
                var tenantProperty = propertyFactory.CreateProperty(TenantPropertyName, user.TenantId);
                logEvent.AddOrUpdateProperty(tenantProperty);
            }
        }
        catch
        {
            // We cannot throw during log enrichment or we would risk endless recursion
        }
    }
}