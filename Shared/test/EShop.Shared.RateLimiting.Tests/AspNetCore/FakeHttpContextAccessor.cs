using Microsoft.AspNetCore.Http;

namespace EShop.Shared.RateLimiting.Tests.AspNetCore;

internal sealed class FakeHttpContextAccessor : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; } = new DefaultHttpContext();
}
