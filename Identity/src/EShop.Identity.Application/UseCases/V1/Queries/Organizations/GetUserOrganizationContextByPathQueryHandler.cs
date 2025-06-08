using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Organizations;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

namespace EShop.Identity.Application.UseCases.V1.Queries.Organizations;

public sealed class GetUserOrganizationContextByPathQueryHandler : IQueryHandler<Query.GetUserOrganizationContextByPathQuery, Response.OrganizationContext>
{
    private readonly IUserOrganizationContextProvider _userOrganizationContextProvider;

    public GetUserOrganizationContextByPathQueryHandler(IUserOrganizationContextProvider userOrganizationContextProvider)
    {
        _userOrganizationContextProvider = userOrganizationContextProvider;
    }

    public async Task<Result<Response.OrganizationContext>> Handle(Query.GetUserOrganizationContextByPathQuery request, CancellationToken cancellationToken)
    {
        var organizationContext = await _userOrganizationContextProvider.GetOrganizationContextByPathAsync(request.OrganizationContextPath);
        return Result.Success(organizationContext);
    }
}