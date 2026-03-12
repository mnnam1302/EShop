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

            var client = await apiContext.GetAuthorizedClient(null);
            var serializedRequest = JsonConvert.SerializeObject(request);
            var httpContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

            using var response = await client.PostAsync($"{BaseUrl}/login", httpContent);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _lastAuthResult = JsonConvert.DeserializeObject<Result<AuthenticationResponse>>(responseBody);

                if (_lastAuthResult?.IsSuccess == true)
                {
                    _lastAuthResponse = _lastAuthResult.Value;
                }
            }
            else
            {
                var json = JObject.Parse(responseBody);
                var errorCode = json.Value<string>("type") ?? "Login";
                var errorMessage = json.Value<string>("detail") ?? "Unknown error";
                _lastAuthResult = Result.Failure<AuthenticationResponse>(new Error(errorCode, errorMessage));
            }

            return _lastAuthResult!;
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

            // For refresh token, we need to pass the access token in the Authorization header
            // The ApiContext should handle this via the user context
            _lastAuthResult = await apiContext.PostAsync<RefreshTokenRequest, AuthenticationResponse>($"{BaseUrl}/refreshToken", request);

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
            // Store the refresh token before logout so we can test it's invalidated
            _previousRefreshToken = _lastAuthResponse?.RefreshToken;

            var request = new LogoutRequest
            {
                UserId = userId
            };

            var user = apiContext.GetUserByUsername(userId);
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
