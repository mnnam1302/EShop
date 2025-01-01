using System.Net;

namespace EShop.Testing.JsonApiApplication;

public sealed class HttpRequestResult<TResource>
    where TResource : class
{
    public HttpStatusCode ResponseStatusCode { get; init; }

    public bool IsSuccessStatusCode
    {
        get
        {
            var statusCode = (int)ResponseStatusCode;
            return statusCode >= 200 && statusCode <= 299;
        }
    }

    public string ReasonPhrase { get; init; }
    public TResource Resource { get; init; }
    public dynamic Included { get; init; }

    public void EnsureSuccessStatusCode()
    {
        if (!IsSuccessStatusCode)
        {
            var message = string.IsNullOrWhiteSpace(ReasonPhrase)
                ? $"Response status code does not indicate success: {(int)ResponseStatusCode}"
                : $"Response status code does not indicate success: {(int)ResponseStatusCode}, ReasonPhrase: '{ReasonPhrase}'";
            throw new HttpRequestException(message);
        }
    }
}