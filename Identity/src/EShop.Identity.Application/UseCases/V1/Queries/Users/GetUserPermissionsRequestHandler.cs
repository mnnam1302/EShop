using AutoMapper;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Users;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;

namespace EShop.Identity.Application.UseCases.V1.Queries.Users;

public class GetUserPermissionsRequestHandler : IQueryHandler<Query.GetUserPermissionsRequest, Response.UserPermissionsResponse>
{
    private readonly IUserPermissionsProvider _userPermissionsProvider;

    public GetUserPermissionsRequestHandler(IUserPermissionsProvider userPermissionsProvider)
    {
        _userPermissionsProvider = userPermissionsProvider;
    }

    public async Task<Result<Response.UserPermissionsResponse>> Handle(Query.GetUserPermissionsRequest request, CancellationToken cancellationToken)
    {
        var permissions = await _userPermissionsProvider.GetPermissions(request.UserId);

        var result = new Response.UserPermissionsResponse
        {
            Permissions = permissions
        };
        return Result.Success(result);
    }
}