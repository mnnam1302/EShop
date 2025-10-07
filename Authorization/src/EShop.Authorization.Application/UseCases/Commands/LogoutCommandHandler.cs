using EShop.Authorization.Application.Abstractions;
using EShop.Authorization.Domain.Constants;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.Scoping.ResourceAccessControl;
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
        var validationResult = await ValidateLogoutRequestAsync(command, cancellationToken);

        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        await RemoveUserSessionAsync(command.UserId, cancellationToken);

        return Result.Success();
    }

    private async Task<Result> ValidateLogoutRequestAsync(LogoutCommand command, CancellationToken cancellationToken)
    {
        // 1. Validate token format and extract user claims
        var tokenValidationResult = await ValidateTokenAndExtractUserIdAsync(command.AccessToken, cancellationToken);

        if (tokenValidationResult.IsFailure)
        {
            _logger.LogWarning("Token validation failed for logout request: {Error}", tokenValidationResult.Error);
            return tokenValidationResult;
        }

        // 2. Verify user ID consistency
        if (command.UserId != tokenValidationResult.Value)
        {
            _logger.LogWarning("User ID mismatch - command: {CommandUserId}, token: {TokenUserId}",
                command.UserId, tokenValidationResult.Value);

            return Result.Failure(ErrorContants.Authentication.InvalidToken);
        }

        // 3. Verify token exists in cache and matches
        var cacheValidationResult = await ValidateTokenInCacheAsync(command, cancellationToken);
        if (cacheValidationResult.IsFailure)
        {
            return cacheValidationResult;
        }

        return Result.Success();
    }

    public async Task<Result<string>> ValidateTokenAndExtractUserIdAsync(string accessToken, CancellationToken cancellationToken)
    {
        var formatValidation = ValidateTokenFormat(accessToken);
        if (formatValidation.IsFailure)
        {
            return Result.Failure<string>(formatValidation.Error);
        }

        var sanitizedToken = JwtEncodedStringHelper.GetJwtEncodedString(accessToken);

        try
        {
            var principal = await _jwtTokenManager.GetPrincipalFromExpiredToken(sanitizedToken, cancellationToken);
            var userId = ExtractUserIdFromClaims(principal);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Token does not contain valid user identifier");
                return Result.Failure<string>(ErrorContants.Authentication.InvalidToken);
            }

            return Result.Success(userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate access token");
            return Result.Failure<string>(ErrorContants.Authentication.InvalidToken);
        }
    }

    public Result ValidateTokenFormat(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Failure(ErrorContants.Authentication.InvalidToken);
        }

        var sanitizedToken = JwtEncodedStringHelper.GetJwtEncodedString(accessToken);
        var tokenParts = sanitizedToken.Split('.');

        if (tokenParts.Length != 3)
        {
            _logger.LogWarning("Token is not in valid JWT format - expected 3 parts, got {PartsCount}", tokenParts.Length);
            return Result.Failure(ErrorContants.Authentication.InvalidToken);
        }

        return Result.Success();
    }

    private static string? ExtractUserIdFromClaims(ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private async Task<Result> ValidateTokenInCacheAsync(LogoutCommand command, CancellationToken cancellationToken)
    {
        var cachedToken = await _userTokenCaching.TryGetTokenAsync(command.UserId, cancellationToken);

        if (cachedToken is null)
        {
            _logger.LogInformation("No cached token found for user {UserId}", command.UserId);
            return Result.Failure(ErrorContants.Authentication.TokenInvalidCache);
        }

        var sanitizedToken = JwtEncodedStringHelper.GetJwtEncodedString(command.AccessToken);

        // TODO: Kodi bug
        // cachedToken.AccessToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjJiNTJlMjMxLWE1NDItNGFhMy05YjA5LTdiZTkxNTc3MDc2YyIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJzeXN0ZW0iLCJ1bmlxdWVfbmFtZSI6IlN5c3RlbSBlU2hvcCIsInRlbmFudF9pZCI6InN5c3RlbSIsInVzZXJfdHlwZSI6InVzZXIiLCJqdGkiOiJmNjcyNTQ4Yy0wNjFlLTQxNGMtYWE1OC0...
        // sanitizedToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjJiNTJlMjMxLWE1NDItNGFhMy05YjA5LTdiZTkxNTc3MDc2YyIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJzeXN0ZW0iLCJ1bmlxdWVfbmFtZSI6IlN5c3RlbSBlU2hvcCIsInRlbmFudF9pZCI6InN5c3RlbSIsInVzZXJfdHlwZSI6InVzZXIiLCJqdGkiOiIwMmI2YWFlZi00MDUzLTQ2MzItOWU5Yi1...
        if (cachedToken.AccessToken != sanitizedToken)
        {
            _logger.LogWarning("Access token mismatch for user {UserId}", command.UserId);
            return Result.Failure(ErrorContants.Authentication.TokenInvalidCache);
        }

        return Result.Success();
    }

    private async Task RemoveUserSessionAsync(string userId, CancellationToken cancellationToken)
    {
        await _userTokenCaching.RemoveCacheAsync(userId, cancellationToken);
    }
}
