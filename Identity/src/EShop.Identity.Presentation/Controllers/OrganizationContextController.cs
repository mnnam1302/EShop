using Asp.Versioning;
using EShop.Shared.Contracts.Services.Identity.Organizations;
using EShop.Shared.JsonApi.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Identity.Presentation.Controllers;

[ApiController]
[Route("api/v{api:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class OrganizationContextController : ApiEndpointBase
{
    private readonly ISender _sender;

    public OrganizationContextController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{id}")]
    public async Task<IResult> GetUserOrganizationContextById([FromRoute] string id)
    {
        var result = await _sender.Send(new Query.GetOrganizationContextByIdQuery(id));
        
        if (result.IsFailure)
        {
            return HandlerFailure(result);
        }

        return TypedResults.Ok(result);
    }

    [HttpGet]
    public async Task<IResult> GetUserOrganizationContextByPath([FromQuery] string organizationContextPath)
    {
        var result = await _sender.Send(new Query.GetUserOrganizationContextByPathQuery(organizationContextPath));
        
        if (result.IsFailure)
        {
            return HandlerFailure(result);
        }

        return TypedResults.Ok(result);
    }
}