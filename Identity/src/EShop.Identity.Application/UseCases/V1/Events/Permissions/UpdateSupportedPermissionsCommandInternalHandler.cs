using EShop.Identity.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Permissions;
using EShop.Shared.DomainTools;
using EShop.Shared.DomainTools.UnitOfWorks;
using Microsoft.Extensions.Logging;

namespace EShop.Identity.Application.UseCases.V1.Events.Permissions;

public class UpdateSupportedPermissionsCommandInternalHandler : ICommandHandler<Command.UpdateSupportedPermissionsCommandInternal>
{
    private readonly IIdentityRepositoryBase<Domain.Entities.Permission, string> _permissionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;
    private readonly IResiliencePolicyFactory _resiliencePolicyFactory;

    public UpdateSupportedPermissionsCommandInternalHandler(
        IIdentityRepositoryBase<Domain.Entities.Permission, string> permissionRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateSupportedPermissionsCommandInternalHandler> logger,
        IResiliencePolicyFactory resiliencePolicyFactory)
    {
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _resiliencePolicyFactory = resiliencePolicyFactory;
    }

    public async Task<Result> Handle(Command.UpdateSupportedPermissionsCommandInternal request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{action} {count} permissions for source system: {source}", request.Action, request.Permissions.Length, request.SourceSystemReference);

        foreach (var permission in request.Permissions)
        {
            var dbPermission = new Domain.Entities.Permission
            {
                Id = permission.Id,
                Name = permission.Name,
                Description = permission.Description,
                RelatedTo = permission.RelatedTo
            };

            _logger.LogDebug("Processing Permissions '{action}' (ID='{id}')", request.Action, dbPermission.Id);

            await _resiliencePolicyFactory
                .CreateDbUpdateHandlingAsyncPolly(_logger)
                .ExecuteAsync(async () =>
                {
                    if (request.Action == SupportedPermissionAction.Added)
                    {
                        await CreateOrUpdatePermissionAsync(dbPermission, cancellationToken);
                    }
                    else
                    {
                        await RemovePermissionAsync(dbPermission, cancellationToken);
                    }
                });
        }

        return Result.Success();
    }

    private async Task CreateOrUpdatePermissionAsync(Domain.Entities.Permission permission, CancellationToken cancellationToken)
    {
        var existingPermission = await _permissionRepository.FindByIdAsync(permission.Id);
        if (existingPermission == null)
        {
            _permissionRepository.Add(permission);
        }
        else
        {
            _permissionRepository.Update(existingPermission);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogTrace("Permission '{permissionId}' added to the system.", permission.Id);
    }

    private async Task RemovePermissionAsync(Domain.Entities.Permission permission, CancellationToken cancellationToken)
    {
        var existingPermission = await _permissionRepository.FindByIdAsync(permission.Id);
        if (existingPermission != null)
        {
            _permissionRepository.Delete(existingPermission);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogTrace("Permission '{permissionId}' removed from the system.", permission.Id);
        }
        else
        {
            _logger.LogWarning("Permission '{permissionId}' is not found in the system.", permission.Id);
        }
    }
}