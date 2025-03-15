namespace EShop.Shared.Scoping.Exceptions;

public class UserOrganizationContextNotFound : NotFoundException
{
    public UserOrganizationContextNotFound(string message)
        : base(message)
    {
    }
}