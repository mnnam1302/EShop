using AutoMapper;
using EShop.Shared.Contracts.Abstractions.Pagination;
using organizationContract = EShop.Shared.Contracts.Services.Identity.Organizations;
using roleContract = EShop.Shared.Contracts.Services.Identity.Roles;

namespace EShop.Identity.Application.Mappers;

public class ProfileService : Profile
{
    public ProfileService()
    {
        CreateMap<Domain.Entities.Role, roleContract.Response.RolesResponse>();
        CreateMap<PagedResult<Domain.Entities.Role>, PagedResult<roleContract.Response.RolesResponse>>();

        CreateMap<Domain.Entities.Organization, organizationContract.Response.OrganizationResponse>();
    }
}