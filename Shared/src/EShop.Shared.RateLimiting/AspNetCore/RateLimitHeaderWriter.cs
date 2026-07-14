using Microsoft.AspNetCore.Http;

namespace EShop.Shared.RateLimiting.AspNetCore;

internal static class RateLimitHeaderWriter
{
    public static void Write(HttpContext? httpContext, long limit, long remaining, long resetSeconds)
    {
        if (httpContext is null || httpContext.Response.HasStarted)
        {
            return;
        }

        httpContext.Response.Headers[RateLimitHeaderNames.Limit] = limit.ToString();
        httpContext.Response.Headers[RateLimitHeaderNames.Remaining] = remaining.ToString();
        httpContext.Response.Headers[RateLimitHeaderNames.Reset] = resetSeconds.ToString();
    }
}
