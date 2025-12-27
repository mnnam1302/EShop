using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace EShop.Authorization.API.APIs;

public static class EndpointHandler
{
    public static IEndpointRouteBuilder MapAuthorizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        endpoints
            .MapAuthEndpoints()
            .MapUserEndpoints()
            .MapOrganizationEndpoints()
            .MapRoleEndpoints();

        return endpoints;
    }
}