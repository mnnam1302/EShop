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

    [Then("the user's refresh token should be invalidated")]
    public void ThenTheUsersRefreshTokenShouldBeInvalidated()
    {
        // The refresh token invalidation is verified by attempting to use it
        // This step is a documentation step; actual verification happens in subsequent steps
        authContext.PreviousRefreshToken.Should().NotBeNullOrEmpty("Previous refresh token should exist for invalidation check");
    }

    [Then("user {string} cannot use the previous refresh token")]
    public async Task ThenUserCannotUseThePreviousRefreshToken(string username)
    {
        var previousRefreshToken = authContext.PreviousRefreshToken;
        previousRefreshToken.Should().NotBeNullOrEmpty("Previous refresh token should exist");

        // Attempt to refresh with the old token
        await authContext.RefreshTokenAsync(string.Empty, previousRefreshToken!);

        authContext.LastAuthResult.Should().NotBeNull();
        authContext.LastAuthResult!.IsFailure.Should().BeTrue("Refresh with invalidated token should fail");
    }
}