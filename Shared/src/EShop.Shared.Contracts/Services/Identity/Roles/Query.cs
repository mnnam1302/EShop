using EShop.Shared.Contracts.Abstractions.Requests;

namespace EShop.Shared.Contracts.Services.Identity.Roles;

public static class Query
{
    public record GetRoles() : IQuery<List<Response.RolesResponse>>;
}