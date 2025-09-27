using EShop.Authorization.Domain.Commands;
using EShop.Authorization.Domain.Repositories;
using EShop.Authorization.Domain.Services;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.UnitOfWorks;

namespace EShop.Authorization.Application.UseCases.Commands;

internal sealed class CreateRootOrganizationCommandHandler : ICommandHandler<CreateRootOrganizationCommand>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateRootOrganizationCommandHandler> _logger;
    private readonly IRootOrganizationService _rootOrganizationService;

    public CreateRootOrganizationCommandHandler(
        IOrganizationRepository organizationRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateRootOrganizationCommandHandler> logger,
        IRootOrganizationService rootOrganizationService)
    {
        _organizationRepository = organizationRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _rootOrganizationService = rootOrganizationService;
    }

    public async Task<Result> HandleAsync(CreateRootOrganizationCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling CreateRootOrganizationCommand for tenant {TenantId}", command.TenantId);

        var setupResult = await _rootOrganizationService.SetupRootOrganizationAsync(
            command.TenantId,
            command.TenantName,
            command.OwnerUsername,
            command.OwnerEmail,
            command.OwnerDisplayName,
            cancellationToken);

        if (setupResult.IsFailure)
        {
            _logger.LogWarning("Failed to setup root organization: {Error}", setupResult.Error);
            return Result.Failure(setupResult.Error);
        }

        var setup = setupResult.Value;
        _organizationRepository.Add(setup.Organization);
        _roleRepository.Add(setup.OwnerRole);
        _userRepository.Add(setup.OwnerUser);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Root organization created successfully for tenant {TenantId}", command.TenantId);
        return Result.Success();
    }
}