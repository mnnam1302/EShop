using EShop.Shared.DomainTools.Extensions;
using FluentAssertions;
using Reqnroll;

namespace EShop.Authorization.Tests.Authentication.Logout;

[Binding]
internal sealed class Steps(AuthenticationStepContext authContext)
{
    [When("user {string} logs out")]
    public async Task WhenUserLogsOut(string username)
    {
        var userId = authContext.LastAuthResponse?.UserId ?? username;
        await authContext.LogoutAsync(userId);
    }

    [Then("the logout should succeed")]
    public void ThenTheLogoutShouldSucceed()
    {
        authContext.LastLogoutResult.Should().NotBeNull();
        authContext.LastLogoutResult!.IsSuccess.Should().BeTrue("Logout should succeed");
    }

    [Then("user {string} cannot use the previous refresh token")]
    public async Task ThenUserCannotUseThePreviousRefreshToken(string username)
    {
        authContext.LastAuthResponse.Should().NotBeNull("There should be a last auth response to get the previous refresh token from");

        // Attempt to refresh with the old token
        await authContext.RefreshTokenAsync(authContext.LastAuthResponse.AccessToken, authContext.LastAuthResponse.RefreshToken);

        authContext.LastAuthResult.Should().NotBeNull();
        authContext.LastAuthResult!.IsFailure.Should().BeTrue("Refresh with invalidated token should fail");
    }
}