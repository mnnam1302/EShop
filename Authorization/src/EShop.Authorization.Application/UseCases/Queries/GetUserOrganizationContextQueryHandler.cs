using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;
using static EShop.Shared.Contracts.Services.Identity.Users.Response;

namespace EShop.Authorization.Application.UseCases.Queries;

public sealed record GetUserOrganizationContextQuery(string UserId) : IQuery<UserOrganizationContext>;

internal class GetUserOrganizationContextQueryHandler : IQueryHandler<GetUserOrganizationContextQuery, UserOrganizationContext>
{
    public Task<Result<UserOrganizationContext>> HandleAsync(GetUserOrganizationContextQuery query, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
