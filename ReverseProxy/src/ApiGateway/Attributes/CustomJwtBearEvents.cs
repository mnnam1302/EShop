using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ApiGateway.Attributes;

public class CustomJwtBearEvents : JwtBearerEvents
{
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly ITokenCachingService _cacheService;

    public CustomJwtBearEvents(IUserDetailsProvider userDetailsProvider, ITokenCachingService cacheService)
    {
        _userDetailsProvider = userDetailsProvider;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Validates the request's token against the cached token in Redis to ensure it is still valid and has not been revoked.
    /// If the token is not found in the cache or does not match the cached token, the request is marked as failed and a header indicating token revocation is added to the response.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var userId = _userDetailsProvider.AuthenticatedUser.Id;
        var requestToken = _userDetailsProvider.GetRawAccessToken();

        requestToken = JwtEncodedStringHelper.GetJwtEncodedString(requestToken);

        var tokenCached = await _cacheService.TryGetTokenAsync(userId);

        if (tokenCached is null ||
            tokenCached.AccessToken is null ||
            tokenCached.AccessToken != requestToken)
        {
            context.HttpContext.Response.Headers.TryAdd("IS-TOKEN-REVOKED", "true");
            context.Fail("Authentication fail. Token has been revoked!");
        }
    }
}