namespace EShop.Shared.Scoping.Exceptions;

public class BadRequestException : DomainException
{
    public BadRequestException(string message)
        : base("Bad Request Exception", message)
    {
    }
}