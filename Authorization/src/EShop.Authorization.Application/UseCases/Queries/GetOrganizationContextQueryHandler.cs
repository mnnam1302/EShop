using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

namespace EShop.Authorization.Application.UseCases.Queries;

public sealed record GetOrganizationContextQuery(string OrganizationId) : IQuery<OrganizationContext>;

internal sealed class GetOrganizationContextQueryHandler : IQueryHandler<GetOrganizationContextQuery, OrganizationContext>
{
    private readonly IUserOrganizationContextProvider _organizationContextProvider;

    public GetOrganizationContextQueryHandler(IUserOrganizationContextProvider organizationContextProvider)
    {
        _organizationContextProvider = organizationContextProvider;
    }

    public async Task<Result<OrganizationContext>> HandleAsync(GetOrganizationContextQuery query, CancellationToken cancellationToken = default)
    {
        var organizationContext = await _organizationContextProvider.GetOrganizationContextForSpecificOrganizationAsync(query.OrganizationId, cancellationToken);

        return Result.Success(organizationContext);
    }
}
