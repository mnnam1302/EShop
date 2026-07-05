using System.Net;
using System.Net.Http;
using EShop.Finance.Application.Services.IntegrationProvider;
using EShop.Finance.Application.Services.IntegrationProvider.Authentication;
using EShop.Finance.Application.Services.IntegrationProvider.Http;
using EShop.Finance.Application.Services.IntegrationProvider.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EShop.Finance.Tests.Application.Integrations;

public class HttpIntegrationClientTests
{
    private static readonly RequestConfiguration CreateInvoice = new()
    {
        Name = "CreateInvoice",
        UrlTemplate = "{{{baseUrl}}}/invoices",
        Method = "POST",
        RequestTemplate = """{ "amount": {{{amount}}} }""",
        ResponseTemplate = """{ "bookingId": "{{{id}}}", "bookingReference": "{{{reference}}}" }""",
    };

    private static readonly Dictionary<string, object?> TemplateData = new()
    {
        ["baseUrl"] = "https://api.example.com",
        ["amount"] = 100m,
    };

    private static AuthenticationOptions NoAuth() =>
        AuthenticationOptions.Create(new Dictionary<string, string?> { ["Scheme"] = "NoAuth" });

    [Fact]
    public async Task Shapes_successful_response_via_response_template()
    {
        using var handler = new StubHandler(HttpStatusCode.OK, """{ "id": "INV-1", "reference": "R-1", "extra": "ignored" }""");
        var client = CreateClient(handler);

        var result = await client.ExecuteRequest(CreateInvoice, TemplateData, "tenant-1", NoAuth(), CancellationToken.None);

        result.Should().NotBeNull();
        result!["bookingId"].Should().Be("INV-1");
        result["bookingReference"].Should().Be("R-1");
        handler.LastRequestBody.Should().Contain("\"amount\": 100");
    }

    [Fact]
    public async Task Returns_null_on_not_found()
    {
       var client = CreateClient(new StubHandler(HttpStatusCode.NotFound, ""));

        var result = await client.ExecuteRequest(CreateInvoice, TemplateData, "tenant-1", NoAuth(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Throws_communication_exception_with_status_on_error()
    {
        var client = CreateClient(new StubHandler(HttpStatusCode.InternalServerError, "boom"));

        var act = () => client.ExecuteRequest(CreateInvoice, TemplateData, "tenant-1", NoAuth(), CancellationToken.None);

        (await act.Should().ThrowAsync<ServerCommunicationException>()).Which.StatusCode.Should().Be(500);
    }

    private static HttpIntegrationClient CreateClient(StubHandler handler)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(HttpIntegrationClient.HttpClientName)).Returns(() => new HttpClient(handler));
        var resolver = new AuthenticationProviderResolver([new NoAuthAuthenticationProvider()]);
        return new HttpIntegrationClient(factory.Object, resolver, NullLogger<HttpIntegrationClient>.Instance);
    }

    private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        public string? LastRequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Content is not null)
            {
                LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            return new HttpResponseMessage(status) { Content = new StringContent(body) };
        }
    }
}
