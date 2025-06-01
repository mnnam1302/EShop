using EShop.Identity.Application.Abstractions;
using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Users;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EShop.Identity.Application.UseCases.V1.Commands.Users;

public class CreateUserCommandHandler : ICommandHandler<Command.CreateUserCommand>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IIdentityRepositoryBase<Role, string> _roleRepository;
    private readonly IIdentityRepositoryBase<User, string> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserDetailsProvider _userDetailsProvider;

    public CreateUserCommandHandler(
        IOrganizationRepository organizationRepository,
        IIdentityRepositoryBase<Role, string> roleRepository,
        IIdentityRepositoryBase<User, string> userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IUserDetailsProvider userDetailsProvider)
    {
        _organizationRepository = organizationRepository;
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _userDetailsProvider = userDetailsProvider;
    }

    public async Task<Result> Handle(Command.CreateUserCommand request, CancellationToken cancellationToken)
    {
        await AssertOrganizationExistingAsync(request.OrganizationId, cancellationToken);
        await AssertDuplicatedUserAsync(request.OrganizationId, request.Username, cancellationToken);

        var roles = await AssertRolesExistingAsync(request.RoleIds, cancellationToken);

        var defaultPassword = _passwordHasher.Hash(Organization.DefaultOwnerPassword);
        var user = User.Create(request.Username, defaultPassword, request.DisplayName, request.Email, _userDetailsProvider.AuthenticatedUser.ActionUserId);
        user.GrantRoles([.. roles.Select(r => r.Id)]);

        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task AssertOrganizationExistingAsync(string organizationId, CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.FindSingleAsync(
            o => o.Id == organizationId,
            cancellationToken: cancellationToken);

        if (organization is not null)
        {
            throw new NotFoundException($"Organization {organizationId} was not found");
        }
    }

    private async Task AssertDuplicatedUserAsync(string organizationId, string username, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.FindSingleAsync(
            x => (x.OrganizationId == organizationId) &&
                 (x.Id == username || x.Username == username),
            cancellationToken: cancellationToken);

        if (existingUser is not null)
        {
            throw new ConflictException($"User {username} already exists in organization {organizationId}");
        }
    }

    private async Task<ICollection<Role>> AssertRolesExistingAsync(string[] roleIds, CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.FindByConditionAsync(
            x => roleIds.Contains(x.Id),
            cancellationToken: cancellationToken);

        if (roles.Count != roleIds.Length)
        {
            var missingRoleIds = roleIds.Except(roles.Select(r => r.Id)).ToArray();
            throw new NotFoundException($"The following roles were not found: {string.Join(", ", missingRoleIds)}");
        }

        return roles;
    }
}