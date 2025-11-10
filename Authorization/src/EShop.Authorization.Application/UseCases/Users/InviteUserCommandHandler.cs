using EShop.Authorization.Application.Services;
using EShop.Authorization.Domain.Constants;
using EShop.Authorization.Domain.Entities;
using EShop.Authorization.Domain.Repositories;
using EShop.Shared.Authentication;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.UnitOfWorks;
using Microsoft.Extensions.Logging;

namespace EShop.Authorization.Application.UseCases.Users;

public sealed class InviteUserCommand : ICommand
{
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
    public string? PhoneNumber { get; init; }
    public required string OrganizationId { get; init; }
    public required Guid[] RoleIds { get; init; } = [];
}

internal sealed class InviteUserCommandHandler(
    IUserRepository userRepository,
    IOrganizationRepository organizationRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ILogger<InviteUserCommandHandler> logger) : ICommandHandler<InviteUserCommand>
{
    public async Task<Result> HandleAsync(InviteUserCommand command, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.FindByIdAsync(command.OrganizationId, cancellationToken: cancellationToken);
        if (organization is null)
        {
            logger.LogWarning("Organization with ID {OrganizationId} not found", command.OrganizationId);
            return Result.Failure(ErrorContants.Organization.NotFound);
        }

        var roles = await roleRepository.FindByConditionAsync(
            r => command.RoleIds.Contains(r.Id),
            cancellationToken: cancellationToken);

        if (roles.Count != command.RoleIds.Length)
        {
            var missingRoleIds = command.RoleIds.Except(roles.Select(r => r.Id)).ToArray();
            logger.LogWarning("The following roles were not found: {MissingRoleIds}", missingRoleIds);

            return Result.Failure(new Error("Role.NotFound", $"The following roles were not found: {string.Join(", ", missingRoleIds)}"));
        }

        var randomPassword = passwordHasher.GenerateRandomPassword();
        var user = User.Create(
            command.Username,
            randomPassword,
            passwordHasher.Hash(randomPassword),
            command.Email,
            command.DisplayName,
            command.OrganizationId,
            UserData.SystemUsername);

        foreach (var roleId in command.RoleIds)
        {
            user.AssignRole(roleId);
        }

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
