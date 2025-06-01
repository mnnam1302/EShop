using EShop.Identity.Application.Abstractions;
using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Organizations;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Identity.Application.UseCases.V1.Events.Tenants;

internal sealed class CreateTenantCommandInternalHandler : ICommandHandler<Command.CreateTenantCommandInternal>
{
    private readonly ILogger _logger;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IIdentityRepositoryBase<Permission, string> _permissionRepository;
    private readonly IIdentityRepositoryBase<Tenant, string> _tenantRepository;
    private readonly IIdentityRepositoryBase<Role, string> _roleRepository;
    private readonly IIdentityRepositoryBase<User, string> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTenantCommandInternalHandler(
        IPasswordHasher passwordHasher,
        ILogger<CreateTenantCommandInternalHandler> logger,
        IUserDetailsProvider userDetailsProvider,
        IOrganizationRepository organizationRepository,
        IIdentityRepositoryBase<Permission, string> permissionRepository,
        IIdentityRepositoryBase<Tenant, string> tenantRepository,
        IIdentityRepositoryBase<Role, string> roleRepository,
        IUnitOfWork unitOfWork,
        IIdentityRepositoryBase<User, string> userRepository)
    {
        _passwordHasher = passwordHasher;
        _logger = logger;
        _userDetailsProvider = userDetailsProvider;
        _organizationRepository = organizationRepository;
        _permissionRepository = permissionRepository;
        _tenantRepository = tenantRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(Command.CreateTenantCommandInternal request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.TenantId) || string.IsNullOrEmpty(request.TenantName))
        {
            return Result.Failure(new Error("Validation.Error", "Tenant ID and Name are required"));
        }

        try
        {
            _userDetailsProvider.SetSystemUserContext(request.TenantId);
            _logger.LogInformation("Creating tenant '{Id}' with name '{Name}'...", request.TenantId, request.TenantName);

            await CreateTenantWithTransactionAsync(request, cancellationToken);
            
            return Result.Success();
        }
        catch (BadRequestException ex)
        {
            _logger.LogWarning(ex, "Validation error creating tenant '{Id}': {Message}", request.TenantId, ex.Message);
            return Result.Failure(new Error("Validation.Error", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant '{Id}' with message: {Message}", request.TenantId, ex.Message);
            return Result.Failure(new Error("Error.CreateTenant", "Failed to create tenant. See logs for details."));
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }

    private async Task CreateTenantWithTransactionAsync(Command.CreateTenantCommandInternal request, CancellationToken cancellationToken)
    {
        // Verify tenant doesn't already exist
        var tenant = await CreateTenantIfNotExistsAsync(request.TenantId, request.TenantName, cancellationToken);

        // Create core tenant entities
        var rootOrganization = CreateRootOrganization(request.TenantId, request.TenantName);
        var ownerRole = await CreateOwnerRoleWithPermissionsAsync(request.TenantId, cancellationToken);
        var ownerUser = CreateOwnerUser(rootOrganization, request);
        ownerUser.GrantRole(ownerRole.Id);

        // Persist entities
        _logger.LogDebug("Saving tenant entities to database...");
        await PersistTenantEntitiesAsync(tenant, rootOrganization, ownerRole, ownerUser, cancellationToken);

        _logger.LogInformation("Successfully created tenant '{Id}' with owner user '{Username}'", request.TenantId, request.OwnerUsername);
    }

    private async Task<Tenant> CreateTenantIfNotExistsAsync(string tenantId, string tenantName, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Verifying tenant '{Id}' does not already exist...", tenantId);

        var existingTenant = await _tenantRepository.FindByIdAsync(tenantId, true, cancellationToken);
        if (existingTenant != null)
        {
            throw new BadRequestException($"Tenant with id '{tenantId}' already exists");
        }

        return new Tenant(tenantId, tenantName);
    }

    private Organization CreateRootOrganization(string tenantId, string tenantName)
    {
        _logger.LogInformation("Creating root organization for tenant '{Id}'...", tenantId);
        return Organization.CreateRootOrganizationInternal(tenantId, tenantName);
    }

    private async Task<Role> CreateOwnerRoleWithPermissionsAsync(string tenantId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating owner role for tenant '{Id}'...", tenantId);

        var ownerRole = Role.Create(Role.OwnerRoleName, "Owner of the account", tenantId);

        var availablePermissionIds = await _permissionRepository
            .FindAll()
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        if (availablePermissionIds.Count == 0)
        {
            _logger.LogWarning("No permissions found to assign to owner role");
        }

        foreach (var permissionId in availablePermissionIds)
        {
            ownerRole.GrantPermission(permissionId);
        }

        return ownerRole;
    }

    private User CreateOwnerUser(Organization organization, Command.CreateTenantCommandInternal request)
    {
        ArgumentNullException.ThrowIfNull(organization);

        _logger.LogDebug("Creating owner user '{Username}' for tenant '{Id}'...",
            request.OwnerUsername, request.TenantId);

        // Security improvement needed: default password should be randomly generated or a password reset flow should be implemented
        var defaultPassword = _passwordHasher.Hash(Organization.DefaultOwnerPassword);

        return User.Create(
            request.OwnerUsername,
            defaultPassword,
            request.OwnerEmail,
            request.OwnerDisplayName,
            organization.Id,
            UserData.SystemUsername);
    }

    private async Task PersistTenantEntitiesAsync(Tenant tenant, Organization rootOrganization, Role ownerRole, User user, CancellationToken cancellationToken)
    {
        _tenantRepository.Add(tenant);
        _organizationRepository.Add(rootOrganization);
        _roleRepository.Add(ownerRole);
        _userRepository.Add(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}