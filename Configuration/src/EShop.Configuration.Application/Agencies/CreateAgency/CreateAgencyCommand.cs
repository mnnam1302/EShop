using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.UnitOfWorks;

namespace EShop.Configuration.Application.Agencies.CreateAgency;

public sealed class CreateAgencyCommand : ICommand
{
    public required string Name { get; init; }
    public required string TenantId { get; init; }
}

public sealed class CreateAgencyCommandHandler : ICommandHandler<CreateAgencyCommand>
{
    private readonly IAgencyRepository _agencyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAgencyCommandHandler(IAgencyRepository agencyRepository, IUnitOfWork unitOfWork)
    {
        _agencyRepository = agencyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(CreateAgencyCommand command, CancellationToken cancellationToken)
    {
        var agency = new Agency(command.Name, command.TenantId);

        var existingAgency = await _agencyRepository.FindSingleAsync(
            x => x.TenantId == agency.TenantId,
            true,
            cancellationToken);
        if (existingAgency is null)
        {
            _agencyRepository.Add(agency);
        }
        else
        {
            UpdateExistingAgency(existingAgency, agency);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private void UpdateExistingAgency(Agency existingAgency, Agency agency)
    {
        existingAgency.Name = agency.Name;
        _agencyRepository.Update(existingAgency);
    }
}