using EShop.Authorization.Domain.Constants;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;

namespace EShop.Authorization.Application.UseCases.Users;

public sealed record GetUserPermissionsQuery(string UserId) : IQuery<IEnumerable<string>>;

internal sealed class GetUserPermissionsQueryHandler : IQueryHandler<GetUserPermissionsQuery, IEnumerable<string>>
{
    private readonly IUserPermissionsProvider _userPermissionsProvider;

    public GetUserPermissionsQueryHandler(IUserPermissionsProvider userPermissionsProvider)
    {
        _userPermissionsProvider = userPermissionsProvider;
    }

    public async Task<Result<IEnumerable<string>>> HandleAsync(GetUserPermissionsQuery query, CancellationToken cancellationToken = default)
    {
        var permissions = await _userPermissionsProvider.GetPermissions(query.UserId, cancellationToken);

        if (permissions == null || permissions.Length == 0)
        {
            return Result.Failure<IEnumerable<string>>(ErrorContants.User.PermissionNotFound);
        }

        return Result.Success(permissions.AsEnumerable());
    }
}
