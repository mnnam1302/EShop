using EShop.Authorization.API.Models;
using EShop.Authorization.Application.UseCases.Organizations;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Authorization.API.APIs;

public static class OrganizationEndpointHandler
{
    private const string BaseUrl = "api/v{version:apiVersion}/organizations";

    public static IEndpointRouteBuilder MapOrganizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .NewVersionedApi("Organizations")
            .MapGroup(BaseUrl)
            .HasApiVersion(1)
            .RequireAuthorization()
            .RequireFeatureFilter(FeatureConstants.Authorization.OrganizationManagement);

        group.MapGet("{organizationId}/organizationContext", GetOrganizationContext)
            .RequireSystemUserFilter();

        group.MapGet("", GetListOrganizations)
            .RequireOneOfPermissionsFilter(
                PermissionConstants.Authorization.ViewOrganizations,
                PermissionConstants.Authorization.ManageOrganizations);

        group.MapPost("{organizationId}/child-organizations", CreateChildOrganization)
            .RequirePermissionFilter(PermissionConstants.Authorization.ManageOrganizations);

        return endpoints;
    }

    private static async Task<IResult> GetOrganizationContext([FromRoute] string organizationId, [FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        var query = new GetOrganizationContextQuery(organizationId);
        var result = await mediator.QueryAsync<GetOrganizationContextQuery, OrganizationContext>(query, cancellationToken);

        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> GetListOrganizations([FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        var query = new GetOrganizationsQuery();
        var result = await mediator.QueryAsync<GetOrganizationsQuery, List<OrganizationsResponse>>(query, cancellationToken);

        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> CreateChildOrganization(
        [FromRoute] string organizationId,
        [FromBody] AddChildOrganizationRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new AddChildOrganizationCommand
        {
            Id = request.Id,
            Name = request.Name,
            Email = request.Email,
            OrganizationNumber = request.OrganizationNumber,
            PhoneNumber = request.PhoneNumber,
            Description = request.Description,
            Street = request.Street,
            City = request.City,
            State = request.State,
            Country = request.Country,
            ZipCode = request.ZipCode,
            ParentOrganizationId = organizationId
        };


        var result = await mediator.SendAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Created("", result);
    }
}
