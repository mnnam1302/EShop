using EShop.Authorization.Domain.Constants;
using EShop.Authorization.Domain.Entities;
using EShop.Authorization.Domain.Repositories;
using EShop.Authorization.Domain.Services;
using EShop.Shared.Authentication;
using EShop.Shared.Contracts.Abstractions.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Authorization.Application.Services;

internal sealed class RootOrganizationService : IRootOrganizationService
{
    private readonly IOrganizationRepository organizationRepository;
    private readonly IPermissionRepository permissionRepository;
    private readonly IPasswordHasher passwordHasher;

    public RootOrganizationService(
        IOrganizationRepository organizationRepository,
        IPermissionRepository permissionRepository,
        IPasswordHasher passwordHasher,
        ILogger<RootOrganizationService> logger)
    {
        this.organizationRepository = organizationRepository;
        this.permissionRepository = permissionRepository;
        this.passwordHasher = passwordHasher;
    }

    public async Task<Result<RootOrganizationCreation>> SetupRootOrganizationAsync(
        string tenantId, string tenantName, string ownerUsername, string ownerEmail, string ownerDisplayName, CancellationToken cancellationToken = default)
    {
        // 1. Validate tenant doesn't already have an organization
        var existingOrganization = await organizationRepository.FindByIdAsync(tenantId, false, cancellationToken);

        if (existingOrganization is not null)
        {
            return Result.Failure<RootOrganizationCreation>(ErrorContants.Organization.AlreadyExists);
        }

        // 2. Create root organization
        var rootOrganization = Organization.CreateRootOrganization(tenantId, tenantName);

        // 3. Get all available permissions for owner role
        var availablePermissions = await permissionRepository
            .FindAll()
            .ToArrayAsync(cancellationToken);

        // 4. Create owner role with all permissions
        var ownerRole = Role.CreateOwnerRole(tenantId);
        ownerRole.GrantPermissions(availablePermissions.Select(p => p.Id));

        // 5. Create owner user
        var ownerUserResult = CreateOwnerUser(
            ownerUsername,
            ownerEmail,
            ownerDisplayName,
            rootOrganization.Id,
            ownerRole.Id);

        if (ownerUserResult.IsFailure)
        {
            return Result.Failure<RootOrganizationCreation>(ownerUserResult.Error);
        }

        var ownerUser = ownerUserResult.Value;

        return Result.Success(new RootOrganizationCreation(rootOrganization, ownerRole, ownerUser));
    }

    private Result<User> CreateOwnerUser(
        string username, string email, string displayName, string organizationId, Guid roleId)
    {
        var temporaryPassword = "Password123!";
        var hashedPassword = passwordHasher.Hash(temporaryPassword);

        var ownerUser = User.Create(
            username,
            temporaryPassword,
            hashedPassword,
            email,
            displayName,
            organizationId,
            UserData.SystemUsername);

        ownerUser.AssignRole(roleId);

        return Result.Success(ownerUser);
    }
}
