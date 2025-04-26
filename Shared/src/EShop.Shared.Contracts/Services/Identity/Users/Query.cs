using EShop.Shared.Contracts.Abstractions.Requests;

namespace EShop.Shared.Contracts.Services.Identity.Users;

public static class Query
{
    public record GetUserOrganizationContextQuery() : IQuery<Response.UserOrganizationContext>;
    public record GetUserPermissionsRequest(string UserId) : IQuery<Response.UserPermissionsResponse>;
}