using EShop.Shared.Authentication.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Threading.RateLimiting;

namespace EShop.Shared.JsonApi.RateLimiting;

public static class RateLimitingExtensions
{
    public static IServiceCollection ConfigureRateLimiters(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.OnRejected = (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                }

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.RequestServices.GetService<ILoggerFactory>()?
                    .CreateLogger("Microsoft.AspNetCore.RateLimitingMiddleware")
                    .LogWarning("OnRejected: {RequestPath}", context.HttpContext.Request.Path);

                return new ValueTask();
            };

            options.AddPolicy("UserBasedRateLimiting", context =>
            {
                var currentUser = context.RequestServices.GetService<IUserDetailsProvider>();

                if (currentUser is not null && currentUser.IsAuthenticatedUser)
                {
                    return RateLimitPartition.GetTokenBucketLimiter(currentUser.AuthenticatedUser.Username,
                        _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 10,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 3,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                            TokensPerPeriod = 4,
                            AutoReplenishment = true
                        });
                }

                return RateLimitPartition.GetSlidingWindowLimiter("anonymous-user",
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 2
                    });
            });

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var currentTenant = context.RequestServices.GetService<IUserDetailsProvider>();

                if (currentTenant is not null && currentTenant.IsAuthenticatedUser)
                {
                    return RateLimitPartition.GetConcurrencyLimiter(currentTenant.AuthenticatedUser.TenantId,
                        _ => new ConcurrencyLimiterOptions
                        {
                            PermitLimit = 5,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 1
                        });
                }

                return RateLimitPartition.GetNoLimiter("host");
            });
        });

        return services;
    }
}