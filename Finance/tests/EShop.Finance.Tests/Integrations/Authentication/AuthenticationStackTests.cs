using System.Net;
using EShop.Finance.Application.Services.IntegrationProvider;
using EShop.Finance.Application.Services.IntegrationProvider.Authentication;
using EShop.Finance.Application.Services.IntegrationProvider.Models;
using EShop.Finance.Infrastructure.Integration.Security;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EShop.Finance.Tests.Integrations.Authentication;

public class AuthenticationStackTests
{
    private static Dictionary<string, string?> OAuthDetails() => new()
    {
        ["Scheme"] = "OAuth",
        ["BaseUrl"] = "https://api.example.com",
        ["ClientId"] = "client-1",
        ["ClientSecret"] = "secret-1",
        ["Scope"] = "accounting/.default",
        ["IdentityServerUrl"] = "https://login.example.com/token",
    };

    [Fact]
    public void Options_default_scheme_is_oauth_and_keys_are_case_insensitive()
    {
        var options = AuthenticationOptions.Create(new Dictionary<string, string?>
        {
            ["clientid"] = "c",
            ["clientsecret"] = "s",
            ["scope"] = "sc",
            ["identityserverurl"] = "https://t",
        });

        options.Scheme.Should().Be("OAuth");
        options.ClientId.Should().Be("c");
        options.OauthAccessTokenEndpoint.Should().Be("https://t");
    }

    [Fact]
    public void Options_reject_oauth_without_required_fields()
    {
        var act = () => AuthenticationOptions.Create(new Dictionary<string, string?> { ["Scheme"] = "OAuth" });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task Basic_provider_applies_basic_header()
    {
        var provider = new BasicAuthenticationProvider();
        var options = AuthenticationOptions.Create(new Dictionary<string, string?>
        {
            ["Scheme"] = "Basic",
            ["Username"] = "user",
            ["Password"] = "pass",
        });
        await provider.Initialize("tenant-1", options, CancellationToken.None);

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com");
        provider.ApplyAuthentication(request);

        request.Headers.Authorization!.Scheme.Should().Be("Basic");
        request.Headers.Authorization.Parameter.Should().Be(Convert.ToBase64String("user:pass"u8.ToArray()));
    }

    [Fact]
    public async Task OAuth_reuses_valid_cached_token_without_calling_endpoint()
    {
        using var handler = new StubHandler();
        var provider = CreateOAuth(handler, out var sessionStore);
        sessionStore.Setup(s => s.GetToken("tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CachedToken("cached-token", DateTimeOffset.UtcNow.AddHours(1)));

        await provider.Initialize("tenant-1", AuthenticationOptions.Create(OAuthDetails()), CancellationToken.None);

        handler.Calls.Should().Be(0);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com");
        provider.ApplyAuthentication(request);
        request.Headers.Authorization!.Parameter.Should().Be("cached-token");
    }

    [Fact]
    public async Task OAuth_refreshes_when_cached_token_expired_and_persists_new_token()
    {
        using var handler = new StubHandler();
        var provider = CreateOAuth(handler, out var sessionStore);
        sessionStore.Setup(s => s.GetToken("tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CachedToken("old-token", DateTimeOffset.UtcNow.AddMinutes(-1)));

        await provider.Initialize("tenant-1", AuthenticationOptions.Create(OAuthDetails()), CancellationToken.None);

        handler.Calls.Should().Be(1);
        sessionStore.Verify(s => s.SaveToken("tenant-1", "fresh-token", It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com");
        provider.ApplyAuthentication(request);
        request.Headers.Authorization!.Parameter.Should().Be("fresh-token");
    }

    [Fact]
    public void Encryptor_round_trips_ciphertext()
    {
        var key = Convert.ToBase64String(new byte[32]);
        var encryptor = new AesFieldEncryptor(Microsoft.Extensions.Options.Options.Create(new AesEncryptionOptions { Key = key }));

        var cipher = encryptor.Encrypt("super-secret-api-key");

        cipher.Should().NotContain("super-secret-api-key");
        encryptor.Decrypt(cipher).Should().Be("super-secret-api-key");
    }

    private static OAuthAuthenticationProvider CreateOAuth(StubHandler handler, out Mock<IProviderSessionStore> sessionStore)
    {
        sessionStore = new Mock<IProviderSessionStore>();
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(OAuthAuthenticationProvider.TokenClientName)).Returns(() => new HttpClient(handler));
        return new OAuthAuthenticationProvider(factory.Object, sessionStore.Object, NullLogger<OAuthAuthenticationProvider>.Instance);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"access_token\":\"fresh-token\",\"expires_in\":3600}"),
            });
        }
    }
}
