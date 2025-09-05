using EShop.Identity.Application.Abstractions;
using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
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
    private readonly IIdentityRepositoryBase<Role, Guid> _roleRepository;
    private readonly IIdentityRepositoryBase<User, string> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTenantCommandInternalHandler(
        ILogger<CreateTenantCommandInternalHandler> logger,
        IPasswordHasher passwordHasher,
        IUserDetailsProvider userDetailsProvider,
        IOrganizationRepository organizationRepository,
        IIdentityRepositoryBase<Permission, string> permissionRepository,
        IIdentityRepositoryBase<Tenant, string> tenantRepository,
        IIdentityRepositoryBase<Role, Guid> roleRepository,
        IIdentityRepositoryBase<User, string> userRepository,
        IUnitOfWork unitOfWork)
    {
        _passwordHasher = passwordHasher;
        _logger = logger;
        _userDetailsProvider = userDetailsProvider;
        _organizationRepository = organizationRepository;
        _permissionRepository = permissionRepository;
        _tenantRepository = tenantRepository;
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(Command.CreateTenantCommandInternal request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating tenant '{Id}' with name '{Name}'...", request.TenantId, request.TenantName);
            _userDetailsProvider.SetSystemUserContext(request.TenantId);

            await CreateTenantAsync(request, cancellationToken);

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

    private async Task CreateTenantAsync(Command.CreateTenantCommandInternal request, CancellationToken cancellationToken)
    {
        var tenant = await CreateTenantIfNotExists(request.TenantId, request.TenantName, cancellationToken);
        var rootOrganization = CreateRootOrganization(request.TenantId, request.TenantName);
        var ownerRole = await CreateOwnerRoleWithPermissions(request.TenantId, cancellationToken);
        var ownerUser = CreateOwnerUser(rootOrganization, request);

        ownerUser.GrantRole(ownerRole.Id);

        _logger.LogDebug("Saving tenant entities to database...");
        await PersistTenantEntitiesAsync(tenant, rootOrganization, ownerRole, ownerUser, cancellationToken);

        _logger.LogInformation("Successfully created tenant '{Id}' with owner user '{Username}'", request.TenantId, request.OwnerUsername);
    }

    private async Task<Tenant> CreateTenantIfNotExists(string tenantId, string tenantName, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Checking if tenant '{Id}' exists...", tenantId);

        var existingTenant = await _tenantRepository.FindByIdAsync(tenantId, true, cancellationToken);
        if (existingTenant != null)
        {
            _logger.LogInformation("Tenant '{Id}' already exists.", tenantId);
            throw new BadRequestException($"Tenant with id '{tenantId}' already exists");
        }

        return new Tenant(tenantId, tenantName);
    }

    private Organization CreateRootOrganization(string tenantId, string tenantName)
    {
        _logger.LogInformation("Creating root organization for tenant '{Id}'...", tenantId);
        return Organization.CreateRootOrganizationInternal(tenantId, tenantName);
    }

    private async Task<Role> CreateOwnerRoleWithPermissions(string tenantId, CancellationToken cancellationToken)
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

        _logger.LogDebug("Creating owner user '{Username}' for tenant '{Id}'...", request.OwnerUsername, request.TenantId);

        // Security improvement needed: default password should be randomly generated or a password reset flow should be implemented
        var hashedPassword = _passwordHasher.Hash(Organization.DefaultOwnerPassword);

        return User.Create(
            request.OwnerUsername,
            hashedPassword,
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