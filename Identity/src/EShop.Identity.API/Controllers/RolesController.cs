using Asp.Versioning;
using EShop.Identity.API.Abstractions;
using EShop.Shared.Contracts.Services.Identity.Roles;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Identity.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class RolesController : ApiEndpoint
    {
        private readonly ISender _sender;

        public RolesController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        [RequirePermission(Permission = PermissionConstants.ManageRolesPermissionId)]
        public async Task<IResult> CreateRole([FromBody] Command.CreateRole command)
        {
            var result = await _sender.Send(command);

            if (result.IsFailure)
            {
                return HandlerFailure(result);
            }

            return Results.Created("", result);
        }

        [HttpGet]
        [RequirePermission(Permission = PermissionConstants.ViewRolesPermissionId)]
        public async Task<IResult> GetRoles()
        {
            var result = await _sender.Send(new Query.GetRoles());

            if (result.IsFailure)
            {
                return HandlerFailure(result);
            }

            return Results.Ok(result);
        }
    }
}