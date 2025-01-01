using EShop.Shared.Contracts.Abstractions.Requests;

namespace EShop.Shared.Contracts.Services.Identity.Users;

public static class Query
{
    public record GetUserPermissionsRequest(string UserId) : IQuery<Response.UserPermissionsResponse>;
}