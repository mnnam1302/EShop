using Eshop.Shared.DomainTools.DomainExceptions;
using Eshop.Shared.DomainTools.UnitOfWorks;
using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Entities;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Users;

namespace EShop.Identity.Application.UseCases.V1.Commands.Users;

public class CreateUserCommandHandler : ICommandHandler<Command.CreateUserCommand>
{
    private readonly IIdentityRepository<Organization, string> _organizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(
        IIdentityRepository<Organization, string> organizationRepository,
        IUnitOfWork unitOfWork)
    {
        _organizationRepository = organizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(Command.CreateUserCommand request, CancellationToken cancellationToken)
    {
        // 1. check organization exists
        // 2. create user
        // 3. add user to organization
        // 4. save changes
        var organization = await _organizationRepository.FindByIdAsync(request.OrganizationId)
            ?? throw new NotFoundException($"Organization was not found");



        return Result.Success();
    }
}