using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Organizations;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.Exceptions;

namespace EShop.Identity.Application.UseCases.V1.Commands.Organizations;

public class CreateOrganizationHandler : ICommandHandler<Command.CreateOrganizationCommand>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrganizationHandler(
        IOrganizationRepository organizationRepository,
        IUnitOfWork unitOfWork)
    {
        _organizationRepository = organizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(Command.CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.ParentOrganizationId))
        {
            throw new BadRequestException("Parent organization ID is required.");
        }

        await AssertNameIsUnique(request.Name);
        await AssertOrganizationNumberIsUnique(request.Name, request.OrganizationNumber);

        var organization = Organization.Create(request);

        // Handle ring-fencing
        var parentOrganization = await _organizationRepository.FindByIdAsync(request.ParentOrganizationId);
        if (parentOrganization == null)
        {
            throw new NotFoundException($"Parent organization with ID {request.ParentOrganizationId} not found.");
        }

        await CheckOrganizationLevel(request.ParentOrganizationId);

        //organization.TenantId = parentOrganization.TenantId;
        var context = OrganisationContext.NewChild(parentOrganization.Context);
        organization.Context = context;
        //organization.Scope = context.Path;

        _organizationRepository.Add(organization);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task AssertNameIsUnique(string name)
    {
        var organization = await _organizationRepository.FindSingleAsync(o => o.Id == name || o.Name == name);
        if (organization != null)
        {
            throw new ConflictException("Organization name is already in use.");
        }
    }

    private async Task AssertOrganizationNumberIsUnique(string id, string organizationNumber)
    {
        var existingOrganization = await _organizationRepository.FindSingleAsync(
            o => o.Id != id && o.OrganizationNumber == organizationNumber);
        if (existingOrganization != null)
        {
            throw new ConflictException("Organization number is already in use.");
        }
    }

    private async Task CheckOrganizationLevel(string parentOrganizationId)
    {
        var parentLevel = await GetOrganizationLevel(parentOrganizationId);
        int nextLevel = parentLevel + 1;

        if (nextLevel > Organization.MaxSupportedLevel)
        {
            throw new BadRequestException($"Organization structure only supports maximum of {Organization.MaxSupportedLevel} levels.");
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