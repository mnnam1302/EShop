namespace EShop.Shared.Scoping.ResourceAccessControl;

public class InvalidRequestException : Exception
{
    public int StatusCode { get; }

    public InvalidRequestException(int statusCode)
        : base("Status: " + statusCode)
    {
        StatusCode = statusCode;
    }
}