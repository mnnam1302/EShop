using EShop.Shared.Contracts.Abstractions.Requests;

namespace EShop.Shared.Contracts.Services.Identity.Organizations;

public static class Query
{
    public record GetOrganizationById(string Id) : IQuery<Response.OrganizationResponse>;
}