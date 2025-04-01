using EShop.Identity.Application.Abstractions;
using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Repositories;
using EShop.Identity.Persistence;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Users;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EShop.Identity.Application.UseCases.V1.Commands.Users;

public class CreateUserCommandHandler : ICommandHandler<Command.CreateUserCommand>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IIdentityRepositoryBase<Role, string> _roleRepository;
    private readonly IIdentityRepositoryBase<User, string> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly UsersDbContext _dbContext;

    public CreateUserCommandHandler(
        IOrganizationRepository organizationRepository,
        IIdentityRepositoryBase<Role, string> roleRepository,
        IIdentityRepositoryBase<User, string> userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IUserDetailsProvider userDetailsProvider,
        UsersDbContext dbContext)
    {
        _organizationRepository = organizationRepository;
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _userDetailsProvider = userDetailsProvider;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(Command.CreateUserCommand request, CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.FindSingleAsync(o => o.Id == request.OrganizationId);
        //var organization = await _dbContext.Organizations.FindAsync(request.OrganizationId, cancellationToken);
        //if (organization is null)
        //{
        //    throw new NotFoundException($"Organization {request.OrganizationId} was not found");
        //}

        var existingUser = await _userRepository
            .FindSingleAsync(x =>
                (x.OrganizationId == request.OrganizationId) &&
                (x.Id == request.Username || x.Username == request.Username));

        if (existingUser is not null)
        {
            throw new ConflictException($"User {request.Username} already exists in organization {request.OrganizationId}");
        }

        var defaultPassword = _passwordHasher.Hash(Organization.DefaultOwnerPassword);
        //var user = organization.AddUser(request.Username, defaultPassword, request.DisplayName, request.Email, _userDetailsProvider.AuthenticatedUser.ActionUserId);

        var roles = await _roleRepository.FindByConditionAsync(x => request.RoleIds.Contains(x.Id));
        if (roles.Count != request.RoleIds.Length)
        {
            throw new NotFoundException("One or more roles were not found");
        }

        //user.GrantRoles(roles.Select(r => r.Id).ToArray());

        //_userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}