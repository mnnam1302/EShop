namespace EShop.Shared.Scoping.Exceptions;

public class UnprocessableEntityException : DomainException
{
    public UnprocessableEntityException(string message) : base("Unproccess Entity Exception", message)
    {
    }
}