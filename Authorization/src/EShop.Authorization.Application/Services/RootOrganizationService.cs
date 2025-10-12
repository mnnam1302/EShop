using EShop.Authorization.Domain.Constants;
using EShop.Authorization.Domain.Entities;
using EShop.Authorization.Domain.Repositories;
using EShop.Authorization.Domain.Services;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Scoping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Authorization.Application.Services;

internal sealed class RootOrganizationService : IRootOrganizationService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<RootOrganizationService> _logger;

    public RootOrganizationService(
        IOrganizationRepository organizationRepository,
        IPermissionRepository permissionRepository,
        IPasswordHasher passwordHasher,
        ILogger<RootOrganizationService> logger)
    {
        _organizationRepository = organizationRepository;
        _permissionRepository = permissionRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result<RootOrganizationSetup>> SetupRootOrganizationAsync(
        string tenantId,
        string tenantName,
        string ownerUsername,
        string ownerEmail,
        string ownerDisplayName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting up root organization for tenant {TenantId}", tenantId);

        // 1. Validate tenant doesn't already have an organization
        var existingOrganization = await _organizationRepository.FindByIdAsync(tenantId, false, cancellationToken);
        if (existingOrganization is not null)
        {
            _logger.LogWarning("Organization with ID {TenantId} already exists", tenantId);
            return Result.Failure<RootOrganizationSetup>(ErrorContants.Organization.AlreadyExists);
        }

        // 2. Create root organization
        var rootOrganization = Organization.CreateRootOrganization(tenantId, tenantName);

        // 3. Get all available permissions for owner role
        var availablePermissions = await _permissionRepository
            .FindAll()
            .ToArrayAsync(cancellationToken);

        if (availablePermissions.Length == 0)
        {
            _logger.LogWarning("No permissions found in the system for tenant {TenantId}", tenantId);
        }

        // 4. Create owner role with all permissions
        var ownerRole = Role.CreateOwnerRole(tenantId);
        ownerRole.GrantPermissions(availablePermissions.Select(p => p.Id));

        _logger.LogInformation(
            "Created owner role with {PermissionCount} permissions for tenant {TenantId}",
            availablePermissions.Length, tenantId);

        // 5. Create owner user
        var ownerUserResult = CreateOwnerUser(
            ownerUsername,
            ownerEmail,
            ownerDisplayName,
            rootOrganization.Id,
            ownerRole.Id);

        if (ownerUserResult.IsFailure)
        {
            return Result.Failure<RootOrganizationSetup>(ownerUserResult.Error);
        }

        var ownerUser = ownerUserResult.Value;

        return Result.Success(new RootOrganizationSetup(rootOrganization, ownerRole, ownerUser));
    }

    private Result<User> CreateOwnerUser(
        string username,
        string email,
        string displayName,
        string organizationId,
        Guid roleId)
    {
        // TODO: Kodi - hard password until implement email service for this first login
        //var temporaryPassword = _passwordHasher.GenerateRandomPassword(12);
        var temporaryPassword = "Password123!";
        var hashedPassword = _passwordHasher.Hash(temporaryPassword);

        var ownerUser = User.Create(
            username,
            temporaryPassword,
            hashedPassword,
            email,
            displayName,
            organizationId,
            UserData.SystemUsername);

        ownerUser.AssignRole(roleId);

        _logger.LogInformation(
            "Created owner user {Username} for organization {OrganizationId}",
            username, organizationId);

        return Result.Success(ownerUser);
    }
}
