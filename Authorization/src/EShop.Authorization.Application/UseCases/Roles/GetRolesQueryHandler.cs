using EShop.Authorization.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;

namespace EShop.Authorization.Application.UseCases.Roles;

public sealed class GetRolesQuery(string? name) : IQuery<List<RoleResponse>>
{
    public string? Name { get; init; } = name;
}

public sealed class RoleResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string TenantId { get; init; } = null!;
}

internal sealed class GetRolesQueryHandler : IQueryHandler<GetRolesQuery, List<RoleResponse>>
{
    private readonly IRoleRepository roleRepository;

    public GetRolesQueryHandler(IRoleRepository roleRepository)
    {
        this.roleRepository = roleRepository;
    }

    public async Task<Result<List<RoleResponse>>> HandleAsync(GetRolesQuery query, CancellationToken cancellationToken = default)
    {
        var roles = string.IsNullOrEmpty(query.Name)
            ? await roleRepository.FindByConditionAsync(cancellationToken: cancellationToken)
            : await roleRepository.FindByConditionAsync(r => r.Name.Contains(query.Name), cancellationToken: cancellationToken);

        var response = roles.Select(role => new RoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            TenantId = role.TenantId
        }).ToList();

        return Result.Success(response);
    }
}
