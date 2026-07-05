namespace EShop.Finance.Application.Services.IntegrationProvider;

public sealed class ServerCommunicationException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
