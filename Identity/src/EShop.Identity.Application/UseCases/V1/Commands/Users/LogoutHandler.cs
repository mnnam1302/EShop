using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Repositories;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Auth;
using EShop.Shared.Scoping.Exceptions;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;

namespace EShop.Identity.Application.UseCases.V1.Commands.Users;

public class LogoutHandler : ICommandHandler<Command.Logout>
{
    private readonly IIdentityRepositoryBase<User, string> _userRepository;
    private readonly IUserTokenCachingService _tokenCachingService;

    public LogoutHandler(IIdentityRepositoryBase<User, string> userRepository,
        IUserTokenCachingService tokenCachingService)
    {
        _userRepository = userRepository;
        _tokenCachingService = tokenCachingService;
    }

    public async Task<Result> Handle(Command.Logout request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindSingleAsync(x => x.Id == request.UserId);
        if (user == null)
        {
            throw new NotFoundException("Invalid request");
        }

        await _tokenCachingService.RemoveCacheAsync(user.Id);
        return Result.Success();
    }
}