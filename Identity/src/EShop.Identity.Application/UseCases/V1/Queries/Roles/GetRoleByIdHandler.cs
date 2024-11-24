using AutoMapper;
using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Exceptions;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Roles;

namespace EShop.Identity.Application.UseCases.V1.Queries.Roles;

public class GetRoleByIdHandler : IQueryHandler<Query.GetRoleById, Response.RolesResponse>
{
    private readonly IRepositoryBase<Role, string> _roleRepository;
    private readonly IMapper _mapper;

    public GetRoleByIdHandler(IRepositoryBase<Role, string> roleRepository, IMapper mapper)
    {
        _roleRepository = roleRepository;
        _mapper = mapper;
    }

    public async Task<Result<Response.RolesResponse>> Handle(Query.GetRoleById request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.FindByIdAsync(request.Id, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException("Role was not found");
        }

        var response = _mapper.Map<Response.RolesResponse>(role);
        return Result.Success(response);
    }
}