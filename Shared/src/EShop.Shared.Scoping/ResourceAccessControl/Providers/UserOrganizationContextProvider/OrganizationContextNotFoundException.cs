using EShop.Shared.Scoping.Exceptions;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

public class OrganizationContextNotFoundException : NotFoundException
{
    public OrganizationContextNotFoundException()
        : base("Organization Context is not found.")
    {
    }
}