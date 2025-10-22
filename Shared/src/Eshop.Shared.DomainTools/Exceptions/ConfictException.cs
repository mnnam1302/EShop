namespace EShop.Shared.DomainTools.Exceptions;

public class ConflictException : DomainException
{
    public ConflictException(string message)
        : base("Conflict Exception", message)
    {
    }
}