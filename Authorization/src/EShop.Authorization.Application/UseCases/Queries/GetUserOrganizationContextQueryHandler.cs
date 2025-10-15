using EShop.Shared.CQRS.Query;

namespace EShop.Authorization.Application.UseCases.Queries;

public sealed record GetUserOrganizationContextQuery(string UserId) : IQuery<UserOrganizationContextResponse>;

internal class GetUserOrganizationContextQueryHandler
{
}
