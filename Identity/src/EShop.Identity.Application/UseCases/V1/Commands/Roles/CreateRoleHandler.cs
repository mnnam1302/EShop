using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Abstractions.UnitOfWorks;
using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Exceptions;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Roles;

namespace EShop.Identity.Application.UseCases.V1.Commands.Roles;

public class CreateRoleHandler : ICommandHandler<Command.CreateRole>
{
    private readonly IRepositoryBase<Role, string> _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleHandler(IRepositoryBase<Role, string> roleRepository, IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(Command.CreateRole request, CancellationToken cancellationToken)
    {
        var existingRole = await _roleRepository.FindSingleAsync(x => x.Name == request.Name);
        if (existingRole != null)
        {
            throw new BadRequestException("Role's name has already exists");
        }

        var role = new Role(Guid.NewGuid(), request.Name, request.Description);

        _roleRepository.Add(role);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}