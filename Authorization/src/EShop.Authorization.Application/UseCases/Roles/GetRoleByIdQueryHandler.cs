using EShop.Authorization.Domain.Constants;
using EShop.Authorization.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;

namespace EShop.Authorization.Application.UseCases.Roles;

public sealed class GetRoleByIdQuery(Guid roleId) : IQuery<RoleDetailsResponse>
{
    public Guid RoleId { get; init; } = roleId;
}

public sealed class RoleDetailsResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string TenantId { get; init; } = null!;
    public required List<RolePermission> Permissions { get; init; } = [];
}

public sealed class RolePermission
{
    public required string PermissionId { get; init; }
    public required string PermissionName { get; init; }
    public string? Description { get; init; }
    public required string RelatedTo { get; init; }
}

internal class GetRoleByIdQueryHandler(IRoleRepository roleRepository) : IQueryHandler<GetRoleByIdQuery, RoleDetailsResponse>
{
    public async Task<Result<RoleDetailsResponse>> HandleAsync(GetRoleByIdQuery query, CancellationToken cancellationToken = default)
    {
        var role = await roleRepository.FindByIdAsync(
            query.RoleId,
            includeProperties: r => r.Permissions,
            cancellationToken: cancellationToken);

        if (role is null)
        {
            return Result.Failure<RoleDetailsResponse>(ErrorContants.Role.NotFound);
        }

        var reponse = new RoleDetailsResponse
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            TenantId = role.TenantId,
            Permissions = role.Permissions.Select(p => new RolePermission
            {
                PermissionId = p.Id,
                PermissionName = p.Name,
                Description = p.Description,
                RelatedTo = p.RelatedTo
            }).ToList()
        };

        return Result.Success(reponse);
    }
}
