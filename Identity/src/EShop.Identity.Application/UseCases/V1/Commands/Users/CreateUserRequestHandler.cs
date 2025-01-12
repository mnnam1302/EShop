using Eshop.Shared.DomainTools.UnitOfWorks;
using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Entities;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Users;

namespace EShop.Identity.Application.UseCases.V1.Commands.Users;

public class CreateUserRequestHandler : ICommandHandler<Command.CreateUserCommand>
{
    private readonly IIdentityRepository<Organization, string> _organizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserRequestHandler(
        IIdentityRepository<Organization, string> organizationRepository,
        IUnitOfWork unitOfWork)
    {
        _organizationRepository = organizationRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<Result> Handle(Command.CreateUserCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}