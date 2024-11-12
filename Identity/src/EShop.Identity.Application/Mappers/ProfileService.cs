using AutoMapper;
using EShop.Shared.Contracts.Services.Identity;

namespace EShop.Identity.Application.Mappers;

public class ProfileService : Profile
{
    public ProfileService()
    {
        CreateMap<Domain.Entities.Role, Response.RolesResponse>();
    }
}