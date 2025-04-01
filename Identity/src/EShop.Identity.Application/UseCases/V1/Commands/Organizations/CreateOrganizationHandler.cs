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
        if (request.ParentOrganizationId != null)
        {
            var foundParentOrganization = await _organizationRepository.FindByIdAsync(request.ParentOrganizationId)
                ?? throw new NotFoundException("Parent organization was not found");
        }

        var organizationExists = await _organizationRepository.FindSingleAsync(x => x.Name == request.Name);
        if (organizationExists != null)
        {
            throw new ConflictException("Organization already exists");
        }

        var organization = Organization.Create(request);

        _organizationRepository.Add(organization);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}