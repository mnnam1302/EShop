namespace Eshop.Shared.DomainTools.DomainExceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string message)
        : base("Not Found Exception", message)
    {
    }
}