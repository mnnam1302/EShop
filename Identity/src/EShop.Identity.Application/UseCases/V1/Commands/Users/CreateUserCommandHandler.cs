using EShop.Identity.Application.Abstractions;
using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Entities;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Users;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.Scoping.Exceptions;

namespace EShop.Identity.Application.UseCases.V1.Commands.Users;

public class CreateUserCommandHandler : ICommandHandler<Command.CreateUserCommand>
{
    private readonly IIdentityAggregateRepository<Organization, string> _organizationRepository;
    private readonly IIdentityRepositoryBase<Role, string> _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(
        IIdentityAggregateRepository<Organization, string> organizationRepository,
        IIdentityRepositoryBase<Role, string> roleRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _organizationRepository = organizationRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(Command.CreateUserCommand request, CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.FindByIdAsync(request.OrganizationId, true);
        if (organization is null)
        {
            throw new NotFoundException("Organization was not found");
        }

        var roles = await _roleRepository.FindByConditionAsync(x => request.RoleIds.Contains(x.Id));
        if (roles.Count() == 0 || roles.Count != request.RoleIds.Count)
        {
            throw new NotFoundException("Roles were not found");
        }

        request.Password = _passwordHasher.Hash("P@ssword123"); // auto-generate and send email, must change pass when first login
        var user = User.Create(request);
        user.GrantRoles(roles.Select(r => r.Id).ToList());

        organization.AddUser(user);
        _organizationRepository.Update(organization);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}