using Asp.Versioning;
using EShop.Identity.API.Abstractions;
using EShop.Shared.Contracts.Services.Identity.Organizations;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Identity.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/organizations")]
    public class OrganizationsController : ApiEndpoint
    {
        private readonly ISender _sender;

        public OrganizationsController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        [RequireSupportUser]
        public async Task<IResult> CreateOrganization([FromBody] Command.CreateOrganization command)
        {
            var result = await _sender.Send(command);

            if (result.IsFailure)
            {
                HandlerFailure(result);
            }

            return Results.Created("", result);
        }

        [HttpGet("{id}")]
        [RequireOneOfPermissions(
            PermissionConstants.ViewOrganizationsPermissionId,
            PermissionConstants.ManageOrganizationsPermissionId
        )]
        public async Task<IResult> GetOrganizationById([FromRoute] string id)
        {
            var result = await _sender.Send(new Query.GetOrganizationById(id));

            if (result.IsFailure)
                return HandlerFailure(result);

            return Results.Ok(result);
        }
    }
}