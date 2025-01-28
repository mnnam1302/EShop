namespace EShop.Shared.DomainTools.DomainExceptions;

public class BadRequestException : DomainException
{
    public BadRequestException(string message)
        : base("Bad Request Exception", message)
    {
    }
}