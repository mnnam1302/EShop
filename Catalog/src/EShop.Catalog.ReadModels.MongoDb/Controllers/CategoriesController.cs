using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Catalog.ReadModels.MongoDb.Controllers;

[RequireFeature(FeatureConstants.Catalog.Product_FeatureId)]
[RequireOneOfPermissions(
    PermissionConstants.Catalog.ManageCategories,
    PermissionConstants.Catalog.ViewCategories)]
public partial class CategoriesController
{
    [HttpGet]
    public override Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        return base.GetAsync(cancellationToken);
    }

    [HttpGet("{id}")]
    public override Task<IActionResult> GetAsync(string id, CancellationToken cancellationToken)
    {
        return base.GetAsync(id, cancellationToken);
    }
}
