using EShop.Authorization.Domain.Commands;
using EShop.Authorization.Domain.Entities;
using EShop.Authorization.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Permissions;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.UnitOfWorks;
using Microsoft.Extensions.Logging;

namespace EShop.Authorization.Application.UseCases.Commands;

internal sealed class UpdateSupportedPermissionsCommandHandler : ICommandHandler<UpdateSupportedPermissionsCommand>
{
    private readonly ILogger<UpdateSupportedPermissionsCommandHandler> _logger;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSupportedPermissionsCommandHandler(
        ILogger<UpdateSupportedPermissionsCommandHandler> logger,
        IPermissionRepository permissionRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(UpdateSupportedPermissionsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{Action} {Count} permissions for source system: {Source}", command.Action, command.Permissions.Length, command.SourceSystemReference);

        foreach (var permission in command.Permissions)
        {
            var permissionModel = Permission.Create(
                permission.Id,
                permission.Name,
                permission.Description,
                permission.RelatedTo);

            _logger.LogDebug("Processing Permissions '{Action}' (ID='{Id}')", command.Action, permissionModel.Id);

            if (command.Action == SupportedPermissionAction.Added)
            {
                await CreateOrUpdatePermissionAsync(permissionModel, cancellationToken);
            }
            else
            {
                await RemovePermissionAsync(permissionModel, cancellationToken);
            }
        }

        return Result.Success();
    }

    private async Task CreateOrUpdatePermissionAsync(Domain.Entities.Permission permission, CancellationToken cancellationToken)
    {
        var existingPermission = await _permissionRepository.FindByIdAsync(permission.Id);
        if (existingPermission is null)
        {
            _permissionRepository.Add(permission);
        }
        else
        {
            _permissionRepository.Update(existingPermission);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogTrace("Permission '{PermissionId}' added to the system.", permission.Id);
    }

    private async Task RemovePermissionAsync(Domain.Entities.Permission permission, CancellationToken cancellationToken)
    {
        var existingPermission = await _permissionRepository.FindByIdAsync(permission.Id);
        if (existingPermission is not null)
        {
            _permissionRepository.Delete(existingPermission);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogTrace("Permission '{PermissionId}' removed from the system.", permission.Id);
        }
        else
        {
            _logger.LogWarning("Permission '{PermissionId}' is not found in the system.", permission.Id);
        }
    }
}
