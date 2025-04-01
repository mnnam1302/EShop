using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Roles;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.Scoping.Exceptions;

namespace EShop.Identity.Application.UseCases.V1.Commands.Roles;

public class DeleteRoleHandler : ICommandHandler<Command.DeleteRole>
{
    private readonly IIdentityRepositoryBase<Role, string> _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoleHandler(IIdentityRepositoryBase<Role, string> roleRepository, IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(Command.DeleteRole request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.FindByIdAsync(request.Id);
        if (role == null)
        {
            throw new NotFoundException("Role is not found");
        }
        
        _roleRepository.Delete(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}