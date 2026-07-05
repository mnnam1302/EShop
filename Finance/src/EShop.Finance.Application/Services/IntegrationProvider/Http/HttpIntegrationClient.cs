using System.Net;
using System.Text;
using EShop.Finance.Application.Services.IntegrationProvider.Authentication;
using EShop.Finance.Application.Services.IntegrationProvider.Configuration;
using Microsoft.Extensions.Logging;

namespace EShop.Finance.Application.Services.IntegrationProvider.Http;

public interface IHttpIntegrationClient
{
    Task<IReadOnlyDictionary<string, string?>?> ExecuteRequest(
        RequestConfiguration request,
        IReadOnlyDictionary<string, object?> templateData,
        string tenantId,
        AuthenticationOptions authOptions,
        CancellationToken cancellationToken);
}

public sealed class HttpIntegrationClient(
    IHttpClientFactory httpClientFactory,
    IAuthenticationProviderResolver authenticationResolver,
    ILogger<HttpIntegrationClient> logger) : IHttpIntegrationClient
{
    public const string HttpClientName = "FinanceHttpIntegration";

    public async Task<IReadOnlyDictionary<string, string?>?> ExecuteRequest(
        RequestConfiguration request,
        IReadOnlyDictionary<string, object?> templateData,
        string tenantId,
        AuthenticationOptions authOptions,
        CancellationToken cancellationToken)
    {
        var method = new HttpMethod(request.Method);
        var url = HandlebarsHelper.Render(request.UrlTemplate, templateData);

        using var httpRequest = new HttpRequestMessage(method, url);

        if (!string.Equals(request.Method, "GET", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(request.RequestTemplate))
        {
            var body = HandlebarsHelper.Render(request.RequestTemplate, templateData);
            httpRequest.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        var authProvider = authenticationResolver.Resolve(authOptions.Scheme);
        await authProvider.Initialize(tenantId, authOptions, cancellationToken);
        authProvider.ApplyAuthentication(httpRequest);

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var response = await client.SendAsync(httpRequest, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogInformation("Provider returned 404 for {Method} request '{Name}'.", request.Method, request.Name);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Provider rejected request '{Name}' with status {Status}.", request.Name, (int)response.StatusCode);
            throw new ServerCommunicationException($"Provider request '{request.Name}' failed with status {(int)response.StatusCode}.", (int)response.StatusCode);
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        if (!string.IsNullOrEmpty(request.ResponseTemplate))
        {
            var responseModel = JsonTemplateData.Parse(content);
            var shaped = HandlebarsHelper.Render(request.ResponseTemplate, responseModel!);
            return JsonTemplateData.FlattenObject(shaped);
        }

        return JsonTemplateData.FlattenObject(content);
    }
}
