using Asp.Versioning;
using EShop.Shared.Contracts.Services.Identity.Users;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Identity.Presentation.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{api:apiVersion}/[controller]")]
public class UserOrganzationContextController
{
    private readonly ISender _sender;

    public UserOrganzationContextController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [RequireAuthenticatedUser]
    public async Task<IResult> GetUserOrganizationContext()
    {
        var result = await _sender.Send(new Query.GetUserOrganizationContextQuery());

        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return TypedResults.Ok(result);
    }
}