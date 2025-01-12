using Eshop.Shared.DomainTools.UnitOfWorks;
using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Exceptions;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Roles;

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
            throw new NotFoundException("Role was not found");
        }

        role.Update(request.Name, request.Description, request.PhoneNumer);

        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}