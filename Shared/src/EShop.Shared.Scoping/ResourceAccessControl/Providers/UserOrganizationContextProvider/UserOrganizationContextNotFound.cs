using EShop.Shared.DomainTools.DomainExceptions;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

public class UserOrganizationContextNotFound : NotFoundException
{
    public UserOrganizationContextNotFound(string message)
        : base(message)
    {
    }
}