using EShop.Authorization.Application.Abstractions;
using EShop.Authorization.Domain.Constants;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace EShop.Authorization.Application.UseCases.Commands;

public sealed class LogoutCommand : ICommand
{
    public required string UserId { get; init; }
    public required string AccessToken { get; init; }
}

internal sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand>
{
    private readonly ILogger<LogoutCommandHandler> _logger;
    private readonly IUserTokenCachingService _userTokenCaching;
    private readonly IJwtTokenManager _jwtTokenManager;

    public LogoutCommandHandler(
        ILogger<LogoutCommandHandler> logger,
        IUserTokenCachingService userTokenCaching,
        IJwtTokenManager jwtTokenManager)
    {
        _logger = logger;
        _userTokenCaching = userTokenCaching;
        _jwtTokenManager = jwtTokenManager;
    }

    public async Task<Result> HandleAsync(LogoutCommand command, CancellationToken cancellationToken)
    {
        // 1. Validate and extract claims from the access token
        var principal = await _jwtTokenManager.GetPrincipalFromExpiredToken(command.AccessToken, cancellationToken);
        var tokenUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(tokenUserId))
        {
            _logger.LogWarning("Token does not contain valid user identifier");
            return Result.Failure(ErrorContants.Authentication.InvalidToken);
        }

        // 2. Verify the user ID matches the token
        if (command.UserId != tokenUserId)
        {
            _logger.LogWarning("User ID mismatch - command UserId: {CommandUserId}, token UserId: {TokenUserId}", command.UserId, tokenUserId);
            return Result.Failure(ErrorContants.Authentication.InvalidToken);
        }

        // 3. Get the user token from cache
        var userTokenCacheValue = await _userTokenCaching.TryGetTokenAsync(command.UserId, cancellationToken);

        if (userTokenCacheValue is null)
        {
            _logger.LogInformation("No cached token found for user {UserId} - user may already be logged out", command.UserId);
            return Result.Failure(ErrorContants.Authentication.TokenInvalidCache);
        }

        // 4. Validate the access token matches the cached token
        if (userTokenCacheValue.AccessToken != command.AccessToken)
        {
            _logger.LogWarning("Access token mismatch for user {UserId}", command.UserId);
            return Result.Failure(ErrorContants.Authentication.TokenInvalidCache);
        }

        // 5. Remove the token from cache
        await _userTokenCaching.RemoveCacheAsync(command.UserId, cancellationToken);

        return Result.Success();
    }
}
