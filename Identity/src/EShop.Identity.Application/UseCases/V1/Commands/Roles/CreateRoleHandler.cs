using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Entities;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Roles;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.Exceptions;

namespace EShop.Identity.Application.UseCases.V1.Commands.Roles;

public class CreateRoleHandler : ICommandHandler<Command.CreateRoleCommand>
{
    private readonly IIdentityRepositoryBase<Role, string> _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserDetailsProvider _userDetailsProvider;

    public CreateRoleHandler(
        IIdentityRepositoryBase<Role, string> roleRepository,
        IUnitOfWork unitOfWork,
        IUserDetailsProvider userDetailsProvider)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _userDetailsProvider = userDetailsProvider;
    }

    public async Task<Result> Handle(Command.CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var existingRole = await _roleRepository.FindSingleAsync(x => x.Name == request.Name);
        if (existingRole != null)
        {
            throw new BadRequestException("Role's name has already exists");
        }

        var role = Role.Create(request.Name,request.Description, _userDetailsProvider.AuthenticatedUser.TenantId);

        _roleRepository.Add(role);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}