using EShop.Authorization.Domain.Entities;
using EShop.Authorization.Domain.Repositories;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.UnitOfWorks;

namespace EShop.Authorization.Application.UseCases.Roles;

public sealed class CreateRoleCommand : ICommand
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required IEnumerable<string> PermissionIds { get; init; }
}

internal sealed class CreateRoleCommandHandler(
    IUserDetailsProvider userDetailsProvider,
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateRoleCommand>
{
    public async Task<Result> HandleAsync(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var existingRole = await roleRepository.FindSingleAsync(
            r => r.Name == command.Name,
            cancellationToken: cancellationToken);

        if (existingRole is not null)
        {
            return Result.Failure(new Error("Role.AlreadyExists", $"A role with the name '{command.Name}' already exists."));
        }

        var permissions = await permissionRepository.FindByConditionAsync(
            p => command.PermissionIds.Contains(p.Id),
            cancellationToken: cancellationToken);

        if (permissions.Count != command.PermissionIds.Count())
        {
            var missingPermissionIds = command.PermissionIds.Except(permissions.Select(p => p.Id)).ToArray();
            return Result.Failure(new Error("Permission.NotFound", $"The following permissions were not found: {string.Join(", ", missingPermissionIds)}"));
        }

        var role = Role.Create(
            command.Name,
            command.Description,
            userDetailsProvider.AuthenticatedUser.TenantId);

        role.GrantPermissions(permissions.Select(p => p.Id));

        roleRepository.Add(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
