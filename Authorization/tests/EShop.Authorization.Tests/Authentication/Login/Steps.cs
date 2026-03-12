using EShop.Authorization.Domain.Entities;
using EShop.Authorization.Domain.Repositories;
using EShop.Authorization.Domain.StateMachines;
using EShop.Authorization.Tests.Setups;
using EShop.Shared.DomainTools.UnitOfWorks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EShop.Authorization.Tests.Authentication.Login;

[Binding]
internal sealed class Steps(ApiContext apiContext, AuthenticationStepContext authContext)
{
    private readonly Dictionary<string, string> _userPasswords = [];

    [Given("user {string} has password {string}")]
    public async Task GivenUserHasPassword(string username, string password)
    {
        _userPasswords[username] = password;

        // Set the password for the user in the database
        await using var scope = apiContext.ServiceProvider.CreateAsyncScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<Application.Services.IPasswordHasher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Poll for user existence - may be created asynchronously by TenantCreatedConsumer
        var user = await WaitForUserAsync(userRepository, username);

        // Set password hash and activate user if pending
        user.PasswordHash = passwordHasher.Hash(password);

        if (user.StateMachine.IsInState(UserState.PendingVerification))
        {
            user.StateMachine.Fire(UserAction.ConfirmInvitation);
        }

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync();
    }

    [Given("user {string} is in {string} state")]
    public async Task GivenUserIsInState(string username, string state)
    {
        await using var scope = apiContext.ServiceProvider.CreateAsyncScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var user = await WaitForUserAsync(userRepository, username);

        if (state == "PendingVerification")
        {
            // Reset to PendingVerification state - this is the default state
            user.Status = nameof(UserState.PendingVerification);
            userRepository.Update(user);
            await unitOfWork.SaveChangesAsync();
        }
    }

    [When("user {string} attempts to log in with password {string}")]
    public async Task WhenUserAttemptsToLogInWithPassword(string username, string password)
    {
        await authContext.LoginAsync(username, password);
    }

    [When("user {string} attempts to log in with password {string} {int} times")]
    public async Task WhenUserAttemptsToLogInWithPasswordMultipleTimes(string username, string password, int times)
    {
        for (int i = 0; i < times; i++)
        {
            await authContext.LoginAsync(username, password);
        }
    }

    [Then("the login should succeed")]
    public void ThenTheLoginShouldSucceed()
    {
        authContext.LastAuthResult.Should().NotBeNull();
        authContext.LastAuthResult!.IsSuccess.Should().BeTrue("Login should succeed");
    }

    [Then("the login should fail with error {string}")]
    public void ThenTheLoginShouldFailWithError(string expectedError)
    {
        authContext.LastAuthResult.Should().NotBeNull();
        authContext.LastAuthResult!.IsFailure.Should().BeTrue("Login should fail");
        authContext.LastAuthResult.Error.Message.Should().Contain(expectedError);
    }

    [Then("the response should contain an access token")]
    public void ThenTheResponseShouldContainAnAccessToken()
    {
        authContext.LastAuthResponse.Should().NotBeNull();
        authContext.LastAuthResponse!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Then("the response should contain a refresh token")]
    public void ThenTheResponseShouldContainARefreshToken()
    {
        authContext.LastAuthResponse.Should().NotBeNull();
        authContext.LastAuthResponse!.RefreshToken.Should().NotBeNullOrEmpty();
    }

    private static async Task<User> WaitForUserAsync(IUserRepository userRepository, string username, int timeoutMs = 10_000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            var user = await userRepository.FindSingleAsync(u => u.Username == username);
            if (user is not null)
            {
                return user;
            }

            await Task.Delay(200);
        }

        throw new TimeoutException($"User '{username}' was not created within {timeoutMs}ms. The async consumer may have failed.");
    }
}
