using EShop.Authorization.Application.Services;
using EShop.Authorization.Domain.Entities;
using EShop.Authorization.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.Scoping;
using Microsoft.EntityFrameworkCore;

namespace EShop.Authorization.Application.UseCases.Organizations.CreateRootOrganization;

public sealed class CreateRootOrganizationCommand : ICommand
{
    public required string TenantId { get; init; }
    public required string TenantName { get; init; }
    public required string OwnerUsername { get; init; }
    public required string OwnerDisplayName { get; init; }
    public required string OwnerEmail { get; init; }
}

public sealed class CreateRootOrganizationCommandHandler : ICommandHandler<CreateRootOrganizationCommand>
{
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRootOrganizationCommandHandler(
        IUserDetailsProvider userDetailsProvider,
        IOrganizationRepository organizationRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _organizationRepository = organizationRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userDetailsProvider = userDetailsProvider;
        _permissionRepository = permissionRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(CreateRootOrganizationCommand command, CancellationToken cancellationToken)
    {
        _userDetailsProvider.SetSystemUserContext(command.TenantId);

        try
        {
            // 1. Create root organization
            var rootOrganization = Organization.CreateRootOrganization(command.TenantId, command.TenantName);

            // 2. Create owner role & grant all permissions to owner role
            var ownerRole = Role.CreateOwnerRole(command.TenantId);

            var allPermissions = await _permissionRepository
                .FindAll()
                .ToArrayAsync(cancellationToken);

            ownerRole.GrantPermissions(allPermissions.Select(p => p.Id));

            // 3. Create owner user & assign owner role to owner user
            //var randomPassword = _passwordHasher.GenerateRandomPassword();
            var randomPassword = $"{command.TenantId}-password123";
            var hashedPassword = _passwordHasher.HashPassword(randomPassword);
            var ownerUser = User.Create(
                command.OwnerUsername,
                randomPassword,
                hashedPassword,
                command.OwnerEmail,
                command.OwnerDisplayName,
                rootOrganization.Id,
                UserData.SystemUsername);

            ownerUser.AssignRole(ownerRole.Id);

            // 4. Save changes
            _organizationRepository.Add(rootOrganization);
            _roleRepository.Add(ownerRole);
            _userRepository.Add(ownerUser);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }
}