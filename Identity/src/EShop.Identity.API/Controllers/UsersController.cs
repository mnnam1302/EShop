using Asp.Versioning;
using EShop.Identity.API.Abstractions;
using EShop.Shared.Contracts.Services.Identity.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Identity.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{api:apiVersion}/[controller]")]
    public class UsersController : ApiEndpoint
    {
        private readonly ISender _sender;

        public UsersController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("register")]
        public async Task<IResult> Register([FromBody] Command.RegisterUser command)
        {
            var result = await _sender.Send(command);

            if (result.IsFailure)
            {
                return HandlerFailure(result);
            }

            return Results.Ok(result);
        }
    }
}
