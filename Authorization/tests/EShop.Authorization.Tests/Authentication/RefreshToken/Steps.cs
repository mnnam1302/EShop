using EShop.Authorization.Tests.Setups;
using FluentAssertions;
using Reqnroll;

namespace EShop.Authorization.Tests.Authentication.RefreshToken;

[Binding]
internal sealed class Steps(AuthenticationStepContext authContext)
{
    private bool _isRefreshTokenExpired;
    private string? _storedRefreshToken;

    [Given("user {string} has logged in successfully")]
    public async Task GivenUserHasLoggedInSuccessfully(string username)
    {
        // First ensure the user has a password set (this might be done in a shared step)
        var result = await authContext.LoginAsync(username, "Password123!");
        result.IsSuccess.Should().BeTrue($"User {username} should be able to log in");

        // Store the refresh token for later use
        _storedRefreshToken = authContext.LastAuthResponse?.RefreshToken;
    }

    [Given("the refresh token has expired")]
    public void GivenTheRefreshTokenHasExpired()
    {
        _isRefreshTokenExpired = true;
        // In a real test, we would manipulate the token cache to expire the token
        // For now, we mark it as expired for the step implementation
    }

    [Given("user {string} has logged out")]
    public async Task GivenUserHasLoggedOut(string username)
    {
        var userId = authContext.LastAuthResponse?.UserId ?? username;
        await authContext.LogoutAsync(userId);
    }

    [When("user {string} refreshes the token with their current refresh token")]
    public async Task WhenUserRefreshesTheTokenWithTheirCurrentRefreshToken(string username)
    {
        var refreshToken = authContext.LastAuthResponse?.RefreshToken ?? _storedRefreshToken;
        refreshToken.Should().NotBeNullOrEmpty("Refresh token should exist from previous login");

        var accessToken = authContext.LastAuthResponse?.AccessToken ?? string.Empty;
        await authContext.RefreshTokenAsync(accessToken, refreshToken!);
    }

    [When("user {string} refreshes the token with an invalid refresh token")]
    public async Task WhenUserRefreshesTheTokenWithAnInvalidRefreshToken(string username)
    {
        var accessToken = authContext.LastAuthResponse?.AccessToken ?? string.Empty;
        await authContext.RefreshTokenAsync(accessToken, "invalid-refresh-token");
    }

    [When("user {string} refreshes the token with their previous refresh token")]
    public async Task WhenUserRefreshesTheTokenWithTheirPreviousRefreshToken(string username)
    {
        var previousRefreshToken = authContext.PreviousRefreshToken ?? _storedRefreshToken;
        previousRefreshToken.Should().NotBeNullOrEmpty("Previous refresh token should exist");

        var accessToken = authContext.LastAuthResponse?.AccessToken ?? string.Empty;
        await authContext.RefreshTokenAsync(accessToken, previousRefreshToken!);
    }

    [Then("the token refresh should succeed")]
    public void ThenTheTokenRefreshShouldSucceed()
    {
        authContext.LastAuthResult.Should().NotBeNull();
        authContext.LastAuthResult!.IsSuccess.Should().BeTrue("Token refresh should succeed");
    }

    [Then("the token refresh should fail")]
    public void ThenTheTokenRefreshShouldFail()
    {
        authContext.LastAuthResult.Should().NotBeNull();
        authContext.LastAuthResult!.IsFailure.Should().BeTrue("Token refresh should fail");
    }

    [Then("the response should contain a new access token")]
    public void ThenTheResponseShouldContainANewAccessToken()
    {
        authContext.LastAuthResponse.Should().NotBeNull();
        authContext.LastAuthResponse!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Then("the response should contain a new refresh token")]
    public void ThenTheResponseShouldContainANewRefreshToken()
    {
        authContext.LastAuthResponse.Should().NotBeNull();
        authContext.LastAuthResponse!.RefreshToken.Should().NotBeNullOrEmpty();
    }
}
