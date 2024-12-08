using EShop.Identity.Application.Abstractions;
using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Abstractions.UnitOfWorks;
using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Exceptions;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Users;
using EShop.Shared.Scoping;

namespace EShop.Identity.Application.UseCases.V1.Commands.Users;

public class RegisterUserHandler : ICommandHandler<Command.RegisterUser>
{
    private readonly IRepositoryBase<User, string> _userRepository;
    private readonly IRepositoryBase<Organization, string> _organizationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserDetailsProvider _userDetailsProvider;

    public RegisterUserHandler(IRepositoryBase<User, string> userRepository,
        IRepositoryBase<Organization, string> organizationRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IUserDetailsProvider userDetailsProvider)
    {
        _userRepository = userRepository;
        _organizationRepository = organizationRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _userDetailsProvider = userDetailsProvider;
    }

    public async Task<Result> Handle(Command.RegisterUser request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.FindSingleAsync(x => x.Username == request.UserName, cancellationToken);
        if (existingUser != null)
        {
            throw new BadRequestException("User's username has already used");
        }

        var user = new User(request.UserName, 
            _passwordHasher.Hash(request.Password),
            request.Email,
            request.DisplayName,
            request.PhoneNumber, 
            request.DateOfBirth);

        if (!string.IsNullOrEmpty(request.OrganizationId))
        {
            var organization = await _organizationRepository.FindSingleAsync(x => x.Id == request.OrganizationId, cancellationToken);
            if (organization == null)
            {
                throw new BadRequestException("Organization is not found");
            }

            user.AssignOrganization(request.OrganizationId);
        }

        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}