using EShop.Identity.Application.Abstractions;
using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Entities;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Users;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.Scoping.Exceptions;

namespace EShop.Identity.Application.UseCases.V1.Commands.Users;

public class RegisterUserHandler : ICommandHandler<Command.RegisterUser>
{
    private readonly IIdentityRepositoryBase<User, string> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserHandler(
        IIdentityRepositoryBase<User, string> userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(Command.RegisterUser request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.FindSingleAsync(x => x.Username == request.UserName);
        if (existingUser != null)
        {
            throw new BadRequestException("User's username has already used");
        }

        var user = new User(
            request.UserName,
            _passwordHasher.Hash(request.Password),
            request.Email,
            request.DisplayName,
            request.OrganizationId);

        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}