using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Organizations;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.Scoping.Exceptions;
using Microsoft.Extensions.Logging;

namespace EShop.Identity.Application.UseCases.V1.Commands.Organizations;

public class CreateOrganizationCommandHandler : ICommandHandler<Command.CreateOrganizationCommand>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateOrganizationCommandHandler(
        IOrganizationRepository organizationRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateOrganizationCommandHandler> logger)
    {
        _organizationRepository = organizationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Temporary solution for Row-Level Security (RLS) applied to the Organization table.
    /// Ensures that the combination of TenantId and Name, or TenantId and Id, is unique.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result> Handle(Command.CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating organization with name: {Name}", request.Name);

        var parentOrganization = await GetParentOrganization(request.ParentOrganizationId);

        await ValidateRequest(request, parentOrganization.TenantId!);

        await ValidateOrganizationHierarchy(parentOrganization);

        var childOrganization = parentOrganization.CreateChildOrganization(request);

        _organizationRepository.Add(childOrganization);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task ValidateRequest(Command.CreateOrganizationCommand request, string tenantId)
    {
        if (string.IsNullOrEmpty(request.ParentOrganizationId))
        {
            throw new BadRequestException("Parent organization ID is required.");
        }

        await AssertNameIsUnique(tenantId, request.Name);

        if (!string.IsNullOrEmpty(request.OrganizationNumber))
        {
            await AssertOrganizationNumberIsUnique(tenantId, request.ParentOrganizationId, request.OrganizationNumber);
        }
    }

    private async Task AssertNameIsUnique(string tenantId, string name)
    {
        var organization = await _organizationRepository
            .FindSingleAsync(o => o.TenantId == tenantId && (o.Id == name || o.Name == name));
        if (organization != null)
        {
            throw new ConflictException($"The organization name '{name}' is already in use within tenant '{tenantId}'.");
        }
    }

    private async Task AssertOrganizationNumberIsUnique(string tenantId, string id, string organizationNumber)
    {
        var existingOrganization = await _organizationRepository.FindSingleAsync(
            o => o.TenantId == tenantId && o.Id != id && o.OrganizationNumber == organizationNumber);
        if (existingOrganization != null)
        {
            throw new ConflictException("Organization number is already in use.");
        }
    }

    private async Task<Organization> GetParentOrganization(string parentOrganizationId)
    {
        var parentOrganization = await _organizationRepository.FindByIdAsync(parentOrganizationId);

        if (parentOrganization == null)
        {
            throw new NotFoundException($"Parent organization with ID {parentOrganizationId} not found.");
        }

        return parentOrganization;
    }

    private async Task ValidateOrganizationHierarchy(Organization parentOrganization)
    {
        var parentLevel = await GetOrganizationLevel(parentOrganization.Id);
        int nextLevel = parentLevel + 1;

        if (nextLevel > Organization.MaxSupportedLevel)
        {
            throw new BadRequestException(
                $"Organization structure only supports maximum of {Organization.MaxSupportedLevel} levels.");
        }
    }

    private async Task<int> GetOrganizationLevel(string organizationId)
    {
        var level = 1;
        var organization = await _organizationRepository.FindByIdAsync(organizationId);

        while (organization != null && organization.ParentOrganizationId != null)
        {
            level++;
            organization = await _organizationRepository.FindByIdAsync(organization.ParentOrganizationId);
        }

        return level;
    }
}