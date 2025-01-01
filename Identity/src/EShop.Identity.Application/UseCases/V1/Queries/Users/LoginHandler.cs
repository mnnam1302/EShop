using EShop.Identity.Application.Abstractions;
using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Exceptions;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Auth;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;

namespace EShop.Identity.Application.UseCases.V1.Queries.Users;

public class LoginHandler : IQueryHandler<Query.Login, Response.AuthenticatedResponse>
{
    private readonly IRepositoryBase<User, string> _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenCachingService _tokenCachingService;

    public LoginHandler(
        IRepositoryBase<User, string> userRepository,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        ITokenCachingService tokenCachingService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _tokenCachingService = tokenCachingService;
    }

    public async Task<Result<Response.AuthenticatedResponse>> Handle(Query.Login request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindSingleAsync(x => x.Username == request.Username);
        if (user == null)
        {
            throw new AuthorizationException("User not found");
        }

        var isMatching = _passwordHasher.Verify(user.PasswordHash, request.Password);
        if (!isMatching)
        {
            throw new AuthorizationException("Invalid password");
        }

        var claims = user.GenerateClaims();
        var accessToken = _tokenService.GenerateAccessToken(claims);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var response = new Response.AuthenticatedResponse()
        {
            UserId = user.Id,
            UserName = user.Username!,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = DateTime.Now.AddHours(6)
        };

        _tokenCachingService.AddToken(user.Id, response);

        return Result.Success(response);
    }
}