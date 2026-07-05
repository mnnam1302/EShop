namespace EShop.Finance.Application.Services.IntegrationProvider;

/// <summary>Raised when the external provider returns a non-transient error. Carries the HTTP status code.</summary>
public sealed class ServerCommunicationException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
