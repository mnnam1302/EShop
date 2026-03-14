using EShop.Authorization.API.Models;
using EShop.Authorization.Application.UseCases.Authentication;
using EShop.Authorization.Tests.Setups;
using EShop.Shared.Contracts.Abstractions.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace EShop.Authorization.Tests.Authentication;

internal sealed class AuthenticationStepContext(ApiContext apiContext)
{
    private const string BaseUrl = "/api/v1/auth";

    private AuthenticationResponse? _lastAuthResponse;
    private Result<AuthenticationResponse>? _lastAuthResult;
    private Result? _lastLogoutResult;
    private string? _previousRefreshToken;
    private string? _lastLoggedInUsername;

    public AuthenticationResponse? LastAuthResponse => _lastAuthResponse;
    public Result<AuthenticationResponse>? LastAuthResult => _lastAuthResult;
    public Result? LastLogoutResult => _lastLogoutResult;
    public string? PreviousRefreshToken => _previousRefreshToken;

    internal async Task<Result<AuthenticationResponse>> LoginAsync(string username, string password)
    {
        try
        {
            var request = new LoginRequest
            {
                Username = username,
                Password = password
            };

            _lastAuthResult = await apiContext.PostAsync<LoginRequest, AuthenticationResponse>($"{BaseUrl}/login", request);

            if (_lastAuthResult.IsSuccess)
            {
                _lastAuthResponse = _lastAuthResult.Value;
                _lastLoggedInUsername = username;
            }

            return _lastAuthResult;
        }
        catch (Exception ex)
        {
            apiContext.LastApiError = ex;
            _lastAuthResult = Result.Failure<AuthenticationResponse>(new("Login", ex.Message));
            return _lastAuthResult;
        }
    }

    internal async Task<Result<AuthenticationResponse>> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        try
        {
            // Store the previous refresh token before refreshing
            _previousRefreshToken = _lastAuthResponse?.RefreshToken;

            var request = new RefreshTokenRequest
            {
                RefreshToken = refreshToken
            };


            _lastAuthResult = await apiContext.PostWithBearerTokenAsync<RefreshTokenRequest, AuthenticationResponse>($"{BaseUrl}/refreshToken", request, accessToken);

            if (_lastAuthResult.IsSuccess)
            {
                _lastAuthResponse = _lastAuthResult.Value;
            }

            return _lastAuthResult;
        }
        catch (Exception ex)
        {
            apiContext.LastApiError = ex;
            return Result.Failure<AuthenticationResponse>(new("RefreshToken", ex.Message));
        }
    }

    internal async Task<Result> LogoutAsync(string userId)
    {
        try
        {
            var request = new LogoutRequest
            {
                UserId = userId
            };

            var user = apiContext.GetUserByUsername(_lastLoggedInUsername ?? userId);
            _lastLogoutResult = await apiContext.PostAsync($"{BaseUrl}/logout", request, user);

            return _lastLogoutResult;
        }
        catch (Exception ex)
        {
            apiContext.LastApiError = ex;
            return Result.Failure(new("Logout", ex.Message));
        }
    }

    internal void ClearAuthResponse()
    {
        _previousRefreshToken = _lastAuthResponse?.RefreshToken;
        _lastAuthResponse = null;
        _lastAuthResult = null;
    }
}
