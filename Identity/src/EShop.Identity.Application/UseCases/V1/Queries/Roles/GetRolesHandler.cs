using AutoMapper;
using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Pagination;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Roles;

namespace EShop.Identity.Application.UseCases.V1.Queries.Roles;

public class GetRolesHandler : IQueryHandler<Query.GetRoles, PagedResult<Response.RolesResponse>>
{
    private readonly IIdentityRepositoryBase<Role, string> _roleRepository;
    private readonly IMapper _mapper;

    public GetRolesHandler(IIdentityRepositoryBase<Role, string> roleRepository, IMapper mapper)
    {
        _roleRepository = roleRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<Response.RolesResponse>>> Handle(Query.GetRoles request, CancellationToken cancellationToken)
    {
        var rolesQuery = string.IsNullOrWhiteSpace(request.Name)
            ? _roleRepository.FindAll()
            : _roleRepository.FindByCondition(x => x.Name!.Contains(request.Name));

        var pagedResult = await PagedResult<Role>.CreateAsync(
            rolesQuery,
            request.Paging.PageIndex,
            request.Paging.PageSize,
            cancellationToken);

        var response = _mapper.Map<PagedResult<Response.RolesResponse>>(pagedResult);
        return Result.Success(response);
    }
}