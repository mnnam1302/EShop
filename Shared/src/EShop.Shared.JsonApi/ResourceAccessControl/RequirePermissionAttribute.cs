using Microsoft.AspNetCore.Mvc.Filters;

namespace EShop.Shared.JsonApi.ResourceAccessControl;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute : Attribute, IFilterFactory
{
    public RequirePermissionAttribute(string permissionId)
    {
        this.Permission = permissionId;
    }

    public string Permission { get; }
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        throw new NotImplementedException();
    }
}