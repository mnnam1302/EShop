using Asp.Versioning;
using EShop.Shared.Contracts.Services.Identity.Organizations;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static EShop.Shared.Scoping.ResourceAccessControl.FeatureConstants;
using static EShop.Shared.Scoping.ResourceAccessControl.PermissionConstants;

namespace EShop.Identity.Presentation.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/organizations")]
public class OrganizationsController
{
    private readonly ISender _sender;

    public OrganizationsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [RequireFeature(Shared.Scoping.ResourceAccessControl.FeatureConstants.Authorization.OrganisationRingFencing)]
    [RequirePermission(Shared.Scoping.ResourceAccessControl.PermissionConstants.Authorization.ManageOrganizations)]
    public async Task<IResult> CreateOrganization([FromBody] Command.CreateOrganizationCommand request)
    {
        var result = await _sender.Send(request);

        if (result.IsFailure)
        {
            ApiResultHandler.HandleFailure(result);
        }

        return Results.Created("", result);
    }

    [HttpPut("{id}")]
    [RequireFeature(Shared.Scoping.ResourceAccessControl.FeatureConstants.Authorization.OrganisationRingFencing)]
    [RequireOneOfPermissions(Shared.Scoping.ResourceAccessControl.PermissionConstants.Authorization.ManageOrganizations)]
    public async Task<IResult> UpdateOrganization([FromRoute] string id, [FromBody] Command.UpdateOrganizationCommand request)
    {
        var command = request with { Id = id };

        var result = await _sender.Send(command);
        if (result.IsFailure)
        {
            ApiResultHandler.HandleFailure(result);
        }

        return Results.Ok(result);
    }

    [HttpGet("{id}")]
    [RequireFeature(Shared.Scoping.ResourceAccessControl.FeatureConstants.Authorization.OrganisationRingFencing)]
    [RequireOneOfPermissions(
        Shared.Scoping.ResourceAccessControl.PermissionConstants.Authorization.ViewOrganizations,
        Shared.Scoping.ResourceAccessControl.PermissionConstants.Authorization.ManageOrganizations)]
    public async Task<IResult> GetOrganizationById([FromRoute] string id)
    {
        var result = await _sender.Send(new Query.GetOrganizationById(id));

        if (result.IsFailure)
            return ApiResultHandler.HandleFailure(result);

        return Results.Ok(result);
    }
}