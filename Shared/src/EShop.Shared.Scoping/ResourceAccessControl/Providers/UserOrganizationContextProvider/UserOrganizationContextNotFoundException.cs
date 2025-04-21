using EShop.Shared.Scoping.Exceptions;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

public class UserOrganizationContextNotFoundException : NotFoundException
{
    public UserOrganizationContextNotFoundException()
        : base("User organization context not found.")
    {
    }
}