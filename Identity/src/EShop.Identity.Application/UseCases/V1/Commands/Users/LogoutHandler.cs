using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Exceptions;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Auth;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;

namespace EShop.Identity.Application.UseCases.V1.Commands.Users;

public class LogoutHandler : ICommandHandler<Command.Logout>
{
    private readonly IRepositoryBase<User, string> _userRepository;
    private readonly ITokenCachingService _tokenCachingService;

    public LogoutHandler(IRepositoryBase<User, string> userRepository,
        ITokenCachingService tokenCachingService)
    {
        _userRepository = userRepository;
        _tokenCachingService = tokenCachingService;
    }

    public async Task<Result> Handle(Command.Logout request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindSingleAsync(x => x.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Invalid request");
        }

        _tokenCachingService.RemoveCache(user.Id);
        return Result.Success();
    }
}