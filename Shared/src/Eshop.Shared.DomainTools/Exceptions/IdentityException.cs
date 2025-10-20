namespace EShop.Shared.DomainTools.Exceptions;

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message)
        : base("Unauthorized Exception", message)
    {
    }
}

public class ForbiddenException : DomainException
{
    public ForbiddenException(string message)
        : base("Forbidden Exception", message)
    {
    }
}