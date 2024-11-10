using Asp.Versioning;
using EShop.Identity.API.Abstractions;
using EShop.Shared.Contracts.Services.Identity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Identity.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    public class AuthenticationController : ApiEndpoint
    {
        private readonly ISender _sender;

        public AuthenticationController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("login")]
        public async Task<IResult> Login([FromBody] Query.Login query)
        {
            var result = await _sender.Send(query);

            if (result.IsFailure)
            {
                return HandlerFailure(result);
            }

            return Results.Ok(result);
        }
    }
}
