namespace Eshop.Shared.DomainTools.DomainExceptions;

public class ConflictException : DomainException
{
    public ConflictException(string message)
        : base("Conflict Exception", message)
    {
    }
}