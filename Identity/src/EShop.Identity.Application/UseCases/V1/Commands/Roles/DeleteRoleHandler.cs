using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Abstractions.UnitOfWorks;
using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Exceptions;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Roles;

namespace EShop.Identity.Application.UseCases.V1.Commands.Roles;

public class DeleteRoleHandler : ICommandHandler<Command.DeleteRole>
{
    private readonly IRepositoryBase<Role, string> _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoleHandler(IRepositoryBase<Role, string> roleRepository, IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(Command.DeleteRole request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.FindByIdAsync(request.Id, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException("Role was not found");
        }

        _roleRepository.Remove(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}