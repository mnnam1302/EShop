using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Cache.Services;
using EShop.Shared.RateLimiting.Abstractions;
using EShop.Shared.RateLimiting.AspNetCore;
using EShop.Shared.RateLimiting.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Model;

namespace EShop.Shared.JsonApi.RateLimiting;

public static class RateLimitingExtensions
{
    private const string PolicyName = "UserBasedRateLimiting";

    // Layer 0 (D8): flat, non-tenant, in-memory guard protecting this node's own capacity. Replaces
    // the old per-tenant ConcurrencyLimiter, which was wrong twice over — concurrency is a per-node
    // resource, not something to partition by tenant, and per-node counters don't cap anything
    // platform-wide anyway once the gateway scales out.
    private const int NodeConcurrencyLimit = 500;

    public static IServiceCollection ConfigureRateLimiters(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                }

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.RequestServices.GetService<ILoggerFactory>()?
                    .CreateLogger("Microsoft.AspNetCore.RateLimitingMiddleware")
                    .LogWarning("OnRejected: {RequestPath}", context.HttpContext.Request.Path);

                context.Lease.TryGetMetadata(RateLimitMetadataNames.ExceededScope, out var exceededScope);

                var errorDocument = new RateLimitErrorDocument
                {
                    Errors =
                    [
                        new RateLimitErrorObject
                        {
                            Status = StatusCodes.Status429TooManyRequests.ToString(NumberFormatInfo.InvariantInfo),
                            Code = "rate_limit_exceeded",
                            Title = "Too Many Requests",
                            Detail = GetRejectionDetail(exceededScope)
                        }
                    ]
                };

                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsJsonAsync(errorDocument, cancellationToken);
            };

            options.AddPolicy(PolicyName, BuildDistributedPartition);

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
                RateLimitPartition.GetConcurrencyLimiter("node", _ => new ConcurrencyLimiterOptions
                {
                    PermitLimit = NodeConcurrencyLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));
        });

        return services;
    }

    private static RateLimitPartition<string> BuildDistributedPartition(HttpContext context)
    {
        var userDetailsProvider = context.RequestServices.GetService<IUserDetailsProvider>();
        var domain = GetDomain(context);

        if (userDetailsProvider is not null && userDetailsProvider.IsAuthenticatedUser)
        {
            var tenantId = userDetailsProvider.AuthenticatedUser.TenantId;
            var userId = userDetailsProvider.AuthenticatedUser.Id;

            return RateLimitPartition.Get(
                $"{tenantId}:{userId}:{domain}",
                _ => CreateTenantAndUserLimiter(context, tenantId, userId, domain));
        }

        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.Get(
            $"ip:{ipAddress}:{domain}",
            _ => CreateAnonymousIpLimiter(context, ipAddress, domain));
    }

    private static RateLimiter CreateTenantAndUserLimiter(HttpContext context, string tenantId, string userId, string domain)
    {
        var rateLimiter = context.RequestServices.GetRequiredService<IRateLimiter>();
        var httpContextAccessor = context.RequestServices.GetRequiredService<IHttpContextAccessor>();

        // This RateLimiter instance is cached and reused by ASP.NET Core for every future request
        // that maps to this same partition key, so these closures must never use `context` directly
        // (it belongs to the one request that happened to create the partition, and is disposed once
        // that request ends). httpContextAccessor.HttpContext resolves the CURRENT ambient request on
        // every call instead, via AsyncLocal — safe to invoke repeatedly across requests.
        TokenBucketCheck ResolveTenantCheck()
        {
            var requestServices = httpContextAccessor.HttpContext!.RequestServices;
            var (tenantPolicy, systemPolicy) = GetCachedPolicies(requestServices, tenantId);
            var rule = ResolveRule(requestServices, tenantPolicy, systemPolicy, domain, RateLimitScopeNames.Tenant);
            return new TokenBucketCheck(
                RateLimitKeyBuilder.TenantQuotaKey(tenantId, domain),
                rule.Burst ?? rule.RequestsPerUnit,
                rule.RequestsPerUnit,
                ToPeriod(rule.Unit));
        }

        TokenBucketCheck ResolveUserCheck()
        {
            var requestServices = httpContextAccessor.HttpContext!.RequestServices;
            var (tenantPolicy, systemPolicy) = GetCachedPolicies(requestServices, tenantId);
            var rule = ResolveRule(requestServices, tenantPolicy, systemPolicy, domain, RateLimitScopeNames.User);
            return new TokenBucketCheck(
                RateLimitKeyBuilder.UserBucketKey(tenantId, userId, domain),
                rule.Burst ?? rule.RequestsPerUnit,
                rule.RequestsPerUnit,
                ToPeriod(rule.Unit));
        }

        return new DistributedTokenBucketRateLimiter(rateLimiter, httpContextAccessor, ResolveTenantCheck, "tenant", ResolveUserCheck, "user");
    }

    private static RateLimiter CreateAnonymousIpLimiter(HttpContext context, string ipAddress, string domain)
    {
        var rateLimiter = context.RequestServices.GetRequiredService<IRateLimiter>();
        var httpContextAccessor = context.RequestServices.GetRequiredService<IHttpContextAccessor>();

        SlidingWindowCheck ResolveIpCheck()
        {
            var requestServices = httpContextAccessor.HttpContext!.RequestServices;
            var (_, systemPolicy) = GetCachedPolicies(requestServices, tenantId: null);
            var rule = ResolveRule(requestServices, tenantPolicy: null, systemPolicy, domain, RateLimitScopeNames.AnonymousIp);
            return new SlidingWindowCheck(
                RateLimitKeyBuilder.AnonymousIpKey(ipAddress, domain),
                rule.RequestsPerUnit,
                ToPeriod(rule.Unit));
        }

        return new DistributedSlidingWindowRateLimiter(rateLimiter, httpContextAccessor, ResolveIpCheck, "ip");
    }

    private static CachedRateLimitRule ResolveRule(
        IServiceProvider requestServices,
        CachedRateLimitPolicy? tenantPolicy,
        CachedRateLimitPolicy? systemPolicy,
        string domain,
        string scope)
    {
        var ruleResolver = requestServices.GetRequiredService<IRateLimitRuleResolver>();
        return ruleResolver.ResolveRule(tenantPolicy, systemPolicy, domain, scope);
    }

    // Both lookups are synchronous, L1-memory-only reads (never Redis/HTTP) so this can safely run
    // inside RateLimitPartition's synchronous factory callback. On a cache miss, the request proceeds
    // using IRateLimitRuleResolver's compiled safety defaults (never blocks), and a background task
    // warms the cache via the existing async path so the next request for this tenant gets real data.
    private static (CachedRateLimitPolicy? TenantPolicy, CachedRateLimitPolicy? SystemPolicy) GetCachedPolicies(IServiceProvider requestServices, string? tenantId)
    {
        var policyResolver = requestServices.GetRequiredService<IRateLimitPolicyResolver>();

        CachedRateLimitPolicy? tenantPolicy = null;
        if (tenantId is not null && !policyResolver.TryGetCachedPolicy(tenantId, out tenantPolicy))
        {
            WarmPolicyCacheInBackground(requestServices, policyResolver, tenantId);
        }

        if (!policyResolver.TryGetCachedPolicy(UserData.SystemTenantId, out var systemPolicy))
        {
            WarmPolicyCacheInBackground(requestServices, policyResolver, UserData.SystemTenantId);
        }

        return (tenantPolicy, systemPolicy);
    }

    private static void WarmPolicyCacheInBackground(IServiceProvider requestServices, IRateLimitPolicyResolver policyResolver, string tenantId)
    {
        var logger = requestServices.GetService<ILoggerFactory>()?.CreateLogger("RateLimitPolicyWarmup");

        _ = Task.Run(async () =>
        {
            try
            {
                await policyResolver.GetPolicy(tenantId);
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Failed to warm rate-limit policy cache for tenant '{TenantId}'", tenantId);
            }
        });
    }

    private static string GetDomain(HttpContext context)
    {
        var routeId = context.Features.Get<IReverseProxyFeature>()?.Route.Config.RouteId;
        if (string.IsNullOrEmpty(routeId))
        {
            return CachedRateLimitRule.AllDomains;
        }

        const string RouteSuffix = "-route";
        return routeId.EndsWith(RouteSuffix, StringComparison.OrdinalIgnoreCase)
            ? routeId[..^RouteSuffix.Length]
            : routeId;
    }

    private static string GetRejectionDetail(string? exceededScope)
    {
        return exceededScope switch
        {
            "tenant" => "The tenant's rate limit quota has been exceeded.",
            "user" => "Your request rate limit has been exceeded.",
            "ip" => "Too many requests from this IP address.",
            _ => "Rate limit exceeded."
        };
    }

    private static TimeSpan ToPeriod(string unit)
    {
        return unit switch
        {
            "Second" => TimeSpan.FromSeconds(1),
            "Minute" => TimeSpan.FromMinutes(1),
            "Hour" => TimeSpan.FromHours(1),
            "Day" => TimeSpan.FromDays(1),
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, "Unknown rate-limit unit.")
        };
    }
}
