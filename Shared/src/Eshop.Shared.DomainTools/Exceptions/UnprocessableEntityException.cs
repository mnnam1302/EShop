namespace EShop.Shared.DomainTools.Exceptions;

public class UnprocessableEntityException : DomainException
{
    public UnprocessableEntityException(string message) : base("Unproccess Entity Exception", message)
    {
    }
}