using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Entities;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Organizations;
using EShop.Shared.DomainTools.DomainExceptions;
using EShop.Shared.DomainTools.UnitOfWorks;

namespace EShop.Identity.Application.UseCases.V1.Commands.Organizations;

public class UpdateOrganizationCommandHandler : ICommandHandler<Command.UpdateOrganizationCommand>
{
    private readonly IIdentityAggregateRepository<Organization, string> _organizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrganizationCommandHandler(
        IIdentityAggregateRepository<Organization, string> organizationRepository, 
        IUnitOfWork unitOfWork)
    {
        _organizationRepository = organizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(Command.UpdateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.FindByIdAsync(request.Id);
        if (organization is null)
        {
            throw new NotFoundException("Organization is not found");
        }

        organization.Update(request);

        _organizationRepository.Update(organization);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}