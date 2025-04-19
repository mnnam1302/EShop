using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Users;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

namespace EShop.Identity.Application.UseCases.V1.Queries.Users;

public sealed class GetUserOrganizationContextQueryHandler : IQueryHandler<Query.GetUserOrganizationContextQuery, Response.UserOrganizationContext>
{
    private readonly IUserOrganizationContextProvider _userOrganizationContextProvider;

    public GetUserOrganizationContextQueryHandler(IUserOrganizationContextProvider userOrganizationContextProvider)
    {
        _userOrganizationContextProvider = userOrganizationContextProvider;
    }

    public async Task<Result<Response.UserOrganizationContext>> Handle(Query.GetUserOrganizationContextQuery request, CancellationToken cancellationToken)
    {
        var userOrganizationContext = await _userOrganizationContextProvider.GetUserOrganizationContextAsync();
        return userOrganizationContext;
    }
}