using EShop.Authorization.Application.UseCases.Queries;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Authorization.API.APIs
{
    public static class OrganizationEndpointHandler
    {
        private const string BaseUrl = "api/v{version:apiVersion}/organizations";

        public static IEndpointRouteBuilder MapOrganizationEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints
                .NewVersionedApi("Organizations")
                .MapGroup(BaseUrl)
                .HasApiVersion(1);

            group.MapGet("/{organizationId}/organizationContext", GetOrganizationContext).RequireAuthenticatedUser();

            return endpoints;
        }

        private static async Task<IResult> GetOrganizationContext(
            [FromRoute] string organizationId,
            [FromServices] IMediator mediator,
            CancellationToken cancellationToken)
        {
            var query = new GetOrganizationContextQuery(organizationId);

            var result = await mediator.QueryAsync<GetOrganizationContextQuery, OrganizationContext>(query, cancellationToken);

            if (result.IsFailure)
            {
                return ApiResultHandler.HandleFailure(result);
            }

            return Results.Ok(result);
        }
    }
}
