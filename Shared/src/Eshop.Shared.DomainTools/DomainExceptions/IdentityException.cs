namespace Eshop.Shared.DomainTools.DomainExceptions;

public class AuthorizationException : DomainException
{
    public AuthorizationException(string message)
        : base("Unauthorization Exception", message)
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