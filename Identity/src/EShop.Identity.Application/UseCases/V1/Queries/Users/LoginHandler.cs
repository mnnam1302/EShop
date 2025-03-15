using EShop.Identity.Application.Abstractions;
using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Entities;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Auth;
using EShop.Shared.Scoping.Exceptions;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;

namespace EShop.Identity.Application.UseCases.V1.Queries.Users;

/// <summary>
/// Existsing a problem, if user is include role, error because role is isolation strategy
/// </summary>
public class LoginHandler : IQueryHandler<Query.Login, Response.AuthenticatedResponse>
{
    private readonly IIdentityRepositoryBase<User, string> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ITokenCachingService _tokenCachingService;

    public LoginHandler(
        IIdentityRepositoryBase<User, string> userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ITokenCachingService tokenCachingService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _tokenCachingService = tokenCachingService;
    }

    public async Task<Result<Response.AuthenticatedResponse>> Handle(Query.Login request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindSingleAsync(x => x.Username == request.Username);
        if (user is null)
        {
            throw new UnauthorizedException("User is not found");
        }

        var isMatching = _passwordHasher.Verify(user.PasswordHash, request.Password);
        if (!isMatching)
        {
            throw new UnauthorizedException("Incorrect password");
        }

        var claims = user.GenerateClaims();
        var accessToken = _tokenService.GenerateAccessToken(claims);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var result = new Response.AuthenticatedResponse()
        {
            UserId = user.Id,
            UserName = user.Username!,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = DateTime.Now.AddHours(6)
        };

        await _tokenCachingService.AddTokenAsync(
            user.Id,
            result);

        return Result.Success(result);
    }
}