using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Cache.Services;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.JsonApi.RateLimiting;
using EShop.Shared.RateLimiting.Abstractions;
using EShop.Shared.RateLimiting.DependencyInjections;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.Redis;

namespace EShop.Shared.JsonApi.Tests.RateLimiting;

public sealed class RateLimitingTestHostFixture : IAsyncLifetime
{
    private RedisContainer _redisContainer = null!;
    private IHost _host = null!;

    public HttpClient Client { get; private set; } = null!;

    public IServiceProvider Services => _host.Services;

    public async Task InitializeAsync()
    {
        _redisContainer = new RedisBuilder().WithImage("redis:7-alpine").Build();
        await _redisContainer.StartAsync();

        var hostBuilder = new HostBuilder()
            // This test host deliberately registers a partial service graph (no Tenancy JWT/auth
            // stack, since these tests bypass the HTTP fallback entirely via pre-seeded Redis+L1
            // cache). The generic host's ValidateOnBuild diagnostic would otherwise try to construct
            // every registered service — including the unused Tenancy HTTP client — at startup.
            .UseDefaultServiceProvider((_, options) =>
            {
                options.ValidateOnBuild = false;
                options.ValidateScopes = false;
            })
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();

                webHost.ConfigureAppConfiguration(config =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["RedisOptions:Enabled"] = "true",
                        ["RedisOptions:Host"] = _redisContainer.GetConnectionString(),
                        ["Services:Tenancy"] = "http://localhost:1",
                        // This fixture exercises the enforced (not shadow) client contract — 429s,
                        // headers, rejection detail — so all layers are enforced by default here.
                        // Shadow-mode behavior itself is covered by DistributedTokenBucketRateLimiterTests
                        // / DistributedSlidingWindowRateLimiterTests, which don't need a full HTTP host.
                        ["RateLimiting:Enforcement:TenantEnforced"] = "true",
                        ["RateLimiting:Enforcement:UserEnforced"] = "true",
                        ["RateLimiting:Enforcement:AnonymousIpEnforced"] = "true"
                    });
                });

                webHost.ConfigureServices((context, services) =>
                {
                    services.AddRouting();
                    services.AddSingleton<ISystemInternalJwtTokenFactory, FakeSystemInternalJwtTokenFactory>();
                    services.AddRedisCacheInfrastructure(context.Configuration);
                    services.AddRateLimitPolicyResolver();
                    services.AddDistributedRateLimiter(context.Configuration);
                    services.AddHttpContextAccessor();
                    services.AddScoped<IUserDetailsProvider, TestUserDetailsProvider>();
                    services.ConfigureRateLimiters();
                });

                webHost.Configure(app =>
                {
                    app.UseRouting();
                    app.UseRateLimiter();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/test", () => Results.Ok())
                            .RequireRateLimiting("UserBasedRateLimiting");
                    });
                });
            });

        _host = await hostBuilder.StartAsync();
        Client = _host.GetTestServer().CreateClient();

        await SeedEmptySystemPolicyAsync();
    }

    // Seeded once so IRateLimitPolicyResolver.TryGetCachedPolicy for the system tenant returns a real
    // (empty) hit instead of missing every time, which would otherwise fire a background warm-up
    // attempt against the fake Tenancy URL on every single request in every test.
    private async Task SeedEmptySystemPolicyAsync()
    {
        var cachingService = Services.GetRequiredService<IRateLimitPolicyCachingService>();
        await cachingService.AddRateLimitPolicy(
            EShop.Shared.Authentication.UserData.SystemTenantId,
            new CachedRateLimitPolicy { HasPolicy = false, Rules = [] });

        var policyResolver = Services.GetRequiredService<IRateLimitPolicyResolver>();
        await policyResolver.GetPolicy(EShop.Shared.Authentication.UserData.SystemTenantId);
    }

    public async Task SeedTenantPolicyAsync(string tenantId, CachedRateLimitPolicy policy)
    {
        var cachingService = Services.GetRequiredService<IRateLimitPolicyCachingService>();
        await cachingService.AddRateLimitPolicy(tenantId, policy);

        var policyResolver = Services.GetRequiredService<IRateLimitPolicyResolver>();
        await policyResolver.GetPolicy(tenantId);
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();

        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        if (_redisContainer is not null)
        {
            await _redisContainer.DisposeAsync();
        }
    }
}

[CollectionDefinition(nameof(RateLimitingTestCollection))]
public sealed class RateLimitingTestCollection : ICollectionFixture<RateLimitingTestHostFixture>
{
}
