using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Entities;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Roles;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.Scoping.Exceptions;

namespace EShop.Identity.Application.UseCases.V1.Commands.Roles;

public class UpdateRoleHandler : ICommandHandler<Command.UpdateRole>
{
    private readonly IIdentityRepositoryBase<Role, string> _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoleHandler(IIdentityRepositoryBase<Role, string> roleRepository, IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(Command.UpdateRole request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.FindByIdAsync(request.Id);
        if (role == null)
        {
            throw new NotFoundException("Role is not found");
        }

        role.Update(request.Name, request.Description);

        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}