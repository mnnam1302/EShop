using AutoMapper;
using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Entities;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity;

namespace EShop.Identity.Application.UseCases.V1.Queries.Roles;

public class GetRolesHandler : IQueryHandler<Query.GetRoles, List<Response.RolesResponse>>
{
    private readonly IRepositoryBase<Role, string> _roleRepository;
    private readonly IMapper _mapper;

    public GetRolesHandler(IRepositoryBase<Role, string> roleRepository, IMapper mapper)
    {
        _roleRepository = roleRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<Response.RolesResponse>>> Handle(Query.GetRoles request, CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.FindAllAsync();

        var response = _mapper.Map<List<Response.RolesResponse>>(roles);
        return Result.Success(response);
    }
}