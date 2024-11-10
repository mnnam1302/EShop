using Asp.Versioning;
using EShop.Identity.API.Abstractions;
using EShop.Shared.Contracts.Services.Identity;
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
        public async Task<IResult> CreateRole([FromBody] Command.CreateRole command)
        {
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return HandlerFailure(result);
            }

            return Results.Created("", result);
        }
    }
}