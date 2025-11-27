using EShop.Authorization.API.Models;
using EShop.Authorization.Application.UseCases.Users;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;
using Microsoft.AspNetCore.Mvc;
using AuhthorizationPermissions = EShop.Shared.Scoping.ResourceAccessControl.PermissionConstants.Authorization;

namespace EShop.Authorization.API.APIs;

public static class UserEndpointHandler
{
    private const string BaseUrl = "api/v{version:apiVersion}/users";

    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .NewVersionedApi("Users")
            .MapGroup(BaseUrl)
            .HasApiVersion(1)
            .RequireAuthorization();

        group.MapGet("/{userId}/permissions", GetUserPermissions)
            .RequireFeatureFilter(FeatureConstants.Authorization.UserInvites)
            .RequireOneOfPermissionsFilter(AuhthorizationPermissions.ManageUsers, AuhthorizationPermissions.ViewUsers);

        group.MapGet("/{userId}/organizationContext", GetUserOrganizationContext)
            .RequireFeatureFilter(FeatureConstants.Authorization.UserInvites)
            .RequireSystemUserFilter();

        group.MapGet("/{userId}", GetUserByIdAsync)
            .RequireFeatureFilter(FeatureConstants.Authorization.UserInvites)
            .RequireOneOfPermissionsFilter(AuhthorizationPermissions.ManageUsers, AuhthorizationPermissions.ViewUsers);

        group.MapPost("", InviteUser)
            .RequireFeatureFilter(FeatureConstants.Authorization.UserInvites)
            .RequirePermissionFilter(AuhthorizationPermissions.ManageUsers);

        group.MapPost("/{userId}/confirmInvitation", ConfirmInvitationAsync)
            .AllowAnonymous();

        return endpoints;
    }

    private static async Task<IResult> GetUserPermissions([FromRoute] string userId, [FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        var query = new GetUserPermissionsQuery(userId);

        var result = await mediator.QueryAsync<GetUserPermissionsQuery, IEnumerable<string>>(query, cancellationToken);

        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> GetUserOrganizationContext([FromRoute] string userId, [FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        var query = new GetUserOrganizationContextQuery(userId);

        var result = await mediator.QueryAsync<GetUserOrganizationContextQuery, UserOrganizationContext>(query, cancellationToken);

        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Ok(result);
    }


    private static async Task<IResult> GetUserByIdAsync([FromRoute] string userId, [FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(userId);

        var result = await mediator.QueryAsync<GetUserByIdQuery, UserDetailsResponse>(query, cancellationToken);

        if (result.IsFailure)
        {
            ApiResultHandler.HandleFailure(result);
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> InviteUser([FromBody] InviteUserRequest request, [FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        var command = new InviteUserCommand
        {
            Username = request.Username,
            Email = request.Email,
            DisplayName = request.DisplayName,
            PhoneNumber = request.PhoneNumber,
            OrganizationId = request.OrganizationId,
            RoleIds = request.RoleIds
        };

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Created("", result);
    }

    private static async Task<IResult> ConfirmInvitationAsync([FromRoute] string userId, [FromBody] ConfirmInvitationRequest request, [FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        var command = new ConfirmInvitationCommand
        {
            Username = userId, // userId is actually the username here
            TemporaryPassword = request.TemporaryPassword,
            NewPassword = request.NewPassword
        };

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Ok();
    }
}
