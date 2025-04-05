using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Organizations;
using EShop.Shared.DomainTools.UnitOfWorks;
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
        var existsingParentOrganization = await _organizationRepository.FindByIdAsync(request.ParentOrganizationId);
        if (existsingParentOrganization == null)
        {
            throw new NotFoundException($"Parent organization with ID {request.ParentOrganizationId} not found.");
        }

        await AssertNameIsUnique(request.Name);

        if (!string.IsNullOrWhiteSpace(request.OrganizationNumber))
        {
            await AssertOrganizationNumberIsUnique(request.Name, request.OrganizationNumber);
        }

        var organization = Organization.Create(request);

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
}