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
using Polly;

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
        try
        {
            _userDetailsProvider.SetSystemUserContext(request.TenantId);
            _logger.LogDebug("Creating owner user for tenant '{id}'...", request.TenantId);

            // 1. Create Tenant
            var tenant = new Tenant
            {
                Id = request.TenantId,
                Name = request.TenantName
            };

            // 2. Create organization
            var rootOrganization = Organization.CreateInternal(request.TenantId, request.TenantName);

            // 3. Create role owner
            _logger.LogDebug("Creating owner role for tenant '{id}'...", request.TenantId);

            var ownerRole = Role.Create(Role.OwnerRoleName, "Owner of the account", request.TenantId);

            var availablePermissionIds = await _permissionRepository.FindAll().Select(p => p.Id).ToListAsync(cancellationToken);
            foreach (var permissionId in availablePermissionIds)
            {
                ownerRole.GrantPermission(permissionId);
            }

            // 4. Create user owner
            var defaultPassword = _passwordHasher.Hash("P@ssword123");
            var ownerUser = User.Create(
                request.OwnerUsername,
                defaultPassword,
                request.OwnerEmail,
                request.OwnerDisplayName,
                request.TenantId,
                UserData.SystemUsername);

            rootOrganization.AddUser(ownerUser);

            ownerUser.GrantRole(ownerRole.Id);

            // 6. Save to database
            _tenantRepository.Add(tenant);
            _roleRepository.Add(ownerRole);
            _organizationRepository.Add(rootOrganization);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant '{id}'", request.TenantId);
            throw new UnprocessableEntityException($"Error creating tenant '{request.TenantId}' with message: {ex.Message}");
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }
}