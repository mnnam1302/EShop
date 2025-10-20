using EShop.Identity.Application.Abstractions;
using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Repositories;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Auth;
using EShop.Shared.Scoping.Exceptions;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;

namespace EShop.Identity.Application.UseCases.V1.Queries.Users;

public class LoginHandler : IQueryHandler<Query.Login, Response.AuthenticatedResponse>
{
    private readonly IIdentityRepositoryBase<User, string> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUserTokenCachingService _tokenCachingService;

    public LoginHandler(
        IIdentityRepositoryBase<User, string> userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUserTokenCachingService tokenCachingService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _tokenCachingService = tokenCachingService;
    }

    public async Task<Result<Response.AuthenticatedResponse>> Handle(Query.Login request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindSingleAsync(x => x.Id == request.Username || x.Username == request.Username);
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

        // Convert to AuthenticationCaching for caching
        var authenticationCaching = new TokenAuthentication
        {
            UserId = result.UserId,
            UserName = result.UserName,
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            RefreshTokenExpiryTime = new DateTimeOffset(result.RefreshTokenExpiryTime)
        };

        await _tokenCachingService.AddTokenAsync(user.Id, authenticationCaching);

        return Result.Success(result);
    }
}