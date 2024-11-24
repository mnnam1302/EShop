using AutoMapper;
using EShop.Shared.Contract.Abstractions.Paging;
using EShop.Shared.Contracts.Services.Identity.Roles;

namespace EShop.Identity.Application.Mappers;

public class ProfileService : Profile
{
    public ProfileService()
    {
        CreateMap<Domain.Entities.Role, Response.RolesResponse>();
        CreateMap<PagedResult<Domain.Entities.Role>, PagedResult<Response.RolesResponse>>();
    }
}