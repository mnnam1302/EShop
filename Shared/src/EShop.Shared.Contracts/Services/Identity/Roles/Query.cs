using EShop.Shared.Contract.Abstractions.Paging;
using EShop.Shared.Contracts.Abstractions.Paging;
using EShop.Shared.Contracts.Abstractions.Requests;

namespace EShop.Shared.Contracts.Services.Identity.Roles;

public static class Query
{
    //public record GetRoles() : IQuery<List<Response.RolesResponse>>;
    public record GetRoles(string? Name, Paging Paging) : IQuery<PagedResult<Response.RolesResponse>>;

    public record GetRoleById(string Id) : IQuery<Response.RolesResponse>;
}