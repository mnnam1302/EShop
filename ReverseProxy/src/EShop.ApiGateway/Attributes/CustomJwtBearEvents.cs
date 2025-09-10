using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace EShop.ApiGateway.Attributes;

public class CustomJwtBearEvents : JwtBearerEvents
{
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly IUserTokenCachingService _cacheService;

    public CustomJwtBearEvents(IUserDetailsProvider userDetailsProvider, IUserTokenCachingService cacheService)
    {
        _userDetailsProvider = userDetailsProvider;
        _cacheService = cacheService;
    }

    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var userId = _userDetailsProvider.AuthenticatedUser.Id;
        var requestToken = _userDetailsProvider.GetRawAccessToken();

        requestToken = JwtEncodedStringHelper.GetJwtEncodedString(requestToken);

        var tokenCached = await _cacheService.TryGetTokenAsync(userId);

        if (tokenCached is null || tokenCached.AccessToken is null || tokenCached.AccessToken != requestToken)
        {
            context.HttpContext.Response.Headers.TryAdd("IS-TOKEN-REVOKED", "true");
            context.Fail("Authentication fail. Token has been revoked!");
        }
    }
}
