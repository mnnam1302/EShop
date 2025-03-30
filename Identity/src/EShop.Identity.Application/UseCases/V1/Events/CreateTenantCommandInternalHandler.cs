using EShop.Identity.Application.Abstractions;
using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Entities;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Identity.Application.UseCases.V1.Events;

public class CreateTenantCommandInternalHandler : ICommandHandler<Command.CreateTenantCommandInternal>
{
    private readonly ILogger _logger;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly IIdentityAggregateRepository<Organization, string> _organizationRepository;
    private readonly IIdentityRepositoryBase<Permission, string> _permissionRepository;
    private readonly IIdentityRepositoryBase<Tenant, string> _tenantRepository;
    private readonly IIdentityRepositoryBase<Role, string> _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTenantCommandInternalHandler(
        IPasswordHasher passwordHasher,
        ILogger<CreateTenantCommandInternalHandler> logger,
        IUserDetailsProvider userDetailsProvider,
        IIdentityAggregateRepository<Organization, string> organizationRepository,
        IIdentityRepositoryBase<Permission, string> permissionRepository,
        IIdentityRepositoryBase<Tenant, string> tenantRepository,
        IIdentityRepositoryBase<Role, string> roleRepository,
        IUnitOfWork unitOfWork)
    {
        _passwordHasher = passwordHasher;
        _logger = logger;
        _userDetailsProvider = userDetailsProvider;
        _organizationRepository = organizationRepository;
        _permissionRepository = permissionRepository;
        _tenantRepository = tenantRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
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
            _logger.LogInformation("Creating tenant '{id}' with name '{name}'...", request.TenantId, request.TenantName);

            var tenant = await CreateTenantInternalAsync(request.TenantId, request.TenantName, cancellationToken);

            var rootOrganization = CreateRootOrganization(request.TenantId, request.TenantName);
            var ownerRole = await CreateOwnerRoleWithPermissionsAsync(request.TenantId, cancellationToken);
            CreateAndValidateOwnerUser(rootOrganization, request);

            _logger.LogDebug("Saving tenant entities to database...");
            _tenantRepository.Add(tenant);
            _roleRepository.Add(ownerRole);
            _organizationRepository.Add(rootOrganization);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created tenant '{id}' with owner user '{username}'", request.TenantId, request.OwnerUsername);
            return Result.Success();
        }
        catch (BadRequestException ex)
        {
            _logger.LogWarning(ex, "Validation error creating tenant '{id}': {message}", request.TenantId, ex.Message);
            return Result.Failure(new Error("Validation.Error", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant '{id}' with message: {message}", request.TenantId, ex.Message);
            return Result.Failure(new Error("Error.CreateTenant", "Failed to create tenant. See logs for details."));
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }

    private async Task<Tenant> CreateTenantInternalAsync(string tenantId, string tenantName, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Verifying tenant '{id}' does not already exist...", tenantId);

        var existingTenant = await _tenantRepository.FindByIdAsync(tenantId, true, cancellationToken);
        if (existingTenant is not null)
        {
            throw new BadRequestException($"Tenant with id '{tenantId}' already exists");
        }

        var tenant = new Tenant
        {
            Id = tenantId,
            Name = tenantName
        };

        return tenant;
    }

    private Organization CreateRootOrganization(string tenantId, string tenantName)
    {
        _logger.LogDebug("Creating root organization for tenant '{id}'...", tenantId);
        return Organization.CreateInternal(tenantId, tenantName);
    }

    private async Task<Role> CreateOwnerRoleWithPermissionsAsync(string tenantId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating owner role for tenant '{id}'...", tenantId);

        var ownerRole = Role.Create(Role.OwnerRoleName, "Owner of the account", tenantId);

        var availablePermissionIds = await _permissionRepository
            .FindAll()
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        foreach (var permissionId in availablePermissionIds)
        {
            ownerRole.GrantPermission(permissionId);
        }

        return ownerRole;
    }

    private void CreateAndValidateOwnerUser(Organization organization, Command.CreateTenantCommandInternal request)
    {
        _logger.LogDebug("Creating owner user '{username}' for tenant '{id}'...", request.OwnerUsername, request.TenantId);

        if (string.IsNullOrEmpty(request.OwnerEmail))
        {
            throw new BadRequestException("Owner email is required");
        }

        var defaultPassword = _passwordHasher.Hash(Organization.DefaultOwnerPassword);

        var ownerUser = organization.AddUser(
            request.OwnerUsername,
            defaultPassword,
            request.OwnerDisplayName,
            request.OwnerEmail,
            UserData.SystemUsername);

        if (ownerUser == null)
        {
            throw new InvalidOperationException($"Failed to create owner user for tenant '{request.TenantId}'");
        }
    }
}