using EShop.Shared.DomainTools.DomainExceptions;
using System.Runtime.Serialization;

namespace EShop.Shared.Scoping.ResourceAccessControl;

public class InvalidRequestException : DomainException
{
    public int StatusCode { get; }

    public InvalidRequestException(int statusCode, string message) 
        : base("Status: " + statusCode, message)
    {
        StatusCode = statusCode;
    }
}