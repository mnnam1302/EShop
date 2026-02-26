namespace EShop.Shared.DomainTools.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string message) 
        : base("Not Found Exception", message)
    {
    }
}