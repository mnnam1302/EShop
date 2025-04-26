using EShop.Shared.Contracts.Abstractions.Requests;

namespace EShop.Shared.Contracts.Services.Identity.Organizations;

public static class Query
{
    public record GetOrganizationContextByIdQuery(string OrganizationId) : IQuery<Response.OrganizationContext>;

    public record GetUserOrganizationContextByPathQuery(string OrganizationContextPath) : IQuery<Response.OrganizationContext>;

    public record GetOrganizationById(string Id) : IQuery<Response.OrganizationResponse>;

}