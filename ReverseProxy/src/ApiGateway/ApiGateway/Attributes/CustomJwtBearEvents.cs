using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
    /// Purpose validate request's token with authentication info cached in Redis. Check token is used or not.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var userId = _userDetailsProvider.AuthenticatedUser.Id;
        var requestToken = _userDetailsProvider.GetRawAccessToken();
        requestToken = JwtEncodedStringHelper.GetJwtEncodedString(requestToken);

        _cacheService.TryGetToken(userId, out var authenticatedCaching);

        if (authenticatedCaching == null ||
            authenticatedCaching?.AccessToken != requestToken)
        {
            context.HttpContext.Response.Headers.TryAdd("IS-TOKEN-REVOKED", "true");
            context.Fail("Authentication fail. Token has been revoked!");
        }
    }
}