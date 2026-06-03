using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

namespace EShop.Authorization.Application.UseCases.Users;

public sealed record GetUserOrganizationContextQuery(string UserId) : IQuery<UserOrganizationContext>;

internal class GetUserOrganizationContextQueryHandler : IQueryHandler<GetUserOrganizationContextQuery, UserOrganizationContext>
{
    private readonly IUserOrganizationContextProvider _userOrganizationContextProvider;

    public GetUserOrganizationContextQueryHandler(IUserOrganizationContextProvider userOrganizationContextProvider)
    {
        _userOrganizationContextProvider = userOrganizationContextProvider;
    }

    public async Task<Result<UserOrganizationContext>> HandleAsync(GetUserOrganizationContextQuery query, CancellationToken cancellationToken = default)
    {
        var userOrganizationContext = await _userOrganizationContextProvider.GetUserOrganizationContextAsync(cancellationToken);

        return Result.Success(userOrganizationContext);
    }
}
