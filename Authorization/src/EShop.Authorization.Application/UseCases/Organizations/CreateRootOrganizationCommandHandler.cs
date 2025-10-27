using EShop.Authorization.Domain.Repositories;
using EShop.Authorization.Domain.Services;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.UnitOfWorks;
using Microsoft.Extensions.Logging;

namespace EShop.Authorization.Application.UseCases.Organizations;

public sealed class CreateRootOrganizationCommand : ICommand
{
    public required string TenantId { get; init; }
    public required string TenantName { get; init; }
    public required string OwnerUsername { get; init; }
    public required string OwnerDisplayName { get; init; }
    public required string OwnerEmail { get; init; }
}

internal sealed class CreateRootOrganizationCommandHandler : ICommandHandler<CreateRootOrganizationCommand>
{
    private readonly IOrganizationRepository organizationRepository;
    private readonly IUserRepository userRepository;
    private readonly IRoleRepository roleRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<CreateRootOrganizationCommandHandler> logger;
    private readonly IRootOrganizationService rootOrganizationService;

    public CreateRootOrganizationCommandHandler(
        IOrganizationRepository organizationRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateRootOrganizationCommandHandler> logger,
        IRootOrganizationService rootOrganizationService)
    {
        this.organizationRepository = organizationRepository;
        this.userRepository = userRepository;
        this.roleRepository = roleRepository;
        this.unitOfWork = unitOfWork;
        this.logger = logger;
        this.rootOrganizationService = rootOrganizationService;
    }

    public async Task<Result> HandleAsync(CreateRootOrganizationCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating root organization for tenant {TenantId}", command.TenantId);

        var setupResult = await rootOrganizationService.SetupRootOrganizationAsync(
            command.TenantId,
            command.TenantName,
            command.OwnerUsername,
            command.OwnerEmail,
            command.OwnerDisplayName,
            cancellationToken);

        if (setupResult.IsFailure)
        {
            logger.LogWarning("Failed to setup root organization: {Error}", setupResult.Error);
            return Result.Failure(setupResult.Error);
        }

        var setup = setupResult.Value;

        organizationRepository.Add(setup.Organization);
        roleRepository.Add(setup.OwnerRole);
        userRepository.Add(setup.OwnerUser);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Root organization created successfully for tenant {TenantId}", command.TenantId);
        return Result.Success();
    }
}
