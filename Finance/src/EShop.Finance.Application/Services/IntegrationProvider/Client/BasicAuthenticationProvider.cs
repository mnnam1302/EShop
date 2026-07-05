using System.Net.Http.Headers;
using System.Text;
using EShop.Finance.Application.Services.IntegrationProvider.Models;

namespace EShop.Finance.Application.Services.IntegrationProvider.Client;

public sealed class BasicAuthenticationProvider : IAuthenticationProvider
{
    private string? _username;
    private string? _password;

    public string Scheme => AuthenticationSchemes.Basic;

    public Task Initialize(string tenantId, AuthenticationOptions options, CancellationToken cancellationToken)
    {
        _username = options.Username;
        _password = options.Password;
        return Task.CompletedTask;
    }

    public void ApplyAuthentication(HttpRequestMessage request)
    {
        var raw = $"{_username ?? string.Empty}:{_password ?? string.Empty}";
        var value = Convert.ToBase64String(Encoding.ASCII.GetBytes(raw));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", value);
    }

    public Task<bool> VerifyAuthentication(AuthenticationOptions options, CancellationToken cancellationToken)
    {
        return Task.FromResult<bool>(!string.IsNullOrEmpty(options.Username) || !string.IsNullOrEmpty(options.Password));
    }
}
