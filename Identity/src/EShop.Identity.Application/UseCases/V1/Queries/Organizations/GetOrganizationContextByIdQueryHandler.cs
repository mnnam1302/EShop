using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Organizations;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

namespace EShop.Identity.Application.UseCases.V1.Queries.Organizations;

public sealed class GetOrganizationContextByIdQueryHandler : IQueryHandler<Query.GetOrganizationContextByIdQuery, Response.OrganizationContext>
{
    private readonly IUserOrganizationContextProvider _userOrganizationContextProvider;

    public GetOrganizationContextByIdQueryHandler(IUserOrganizationContextProvider userOrganizationContextProvider)
    {
        _userOrganizationContextProvider = userOrganizationContextProvider;
    }

    public async Task<Result<Response.OrganizationContext>> Handle(Query.GetOrganizationContextByIdQuery request, CancellationToken cancellationToken)
    {
        var organizationContext = await _userOrganizationContextProvider.GetOrganizationContextForSpecificOrganizationAsync(request.OrganizationId);
        return Result.Success(organizationContext);
    }
}