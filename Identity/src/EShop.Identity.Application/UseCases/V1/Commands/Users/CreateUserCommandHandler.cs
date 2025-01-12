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
    private readonly IIdentityAggregateRepository<Organization, string> _organizationRepository;
    private readonly IIdentityRepositoryBase<Role, string> _roleRepository; 
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(
        IIdentityAggregateRepository<Organization, string> organizationRepository,
        IIdentityRepositoryBase<Role, string> roleRepository,
        IUnitOfWork unitOfWork)
    {
        _organizationRepository = organizationRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(Command.CreateUserCommand request, CancellationToken cancellationToken)
    {
        // 1. check organization exists
        // 2. check roleIds exists
        // 3. create user
        // 4. add roles to user
        // 5. add user to organization
        // 6. save changes

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

        var user = User.Create(request);
        user.AddRoles(roles.Select(r => r.Id).ToList());
        
        organization.AddUser(user);
        _organizationRepository.Update(organization);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}