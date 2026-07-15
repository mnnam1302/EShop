using EShop.Shared.Cache.Services;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EShop.Shared.JsonApi.Tests.RateLimiting;

[Collection(nameof(RateLimitingTestCollection))]
public sealed class RateLimitingIntegrationTests(RateLimitingTestHostFixture fixture)
{
    [Fact]
    public async Task Tenant_Quota_Exceeded_Returns_429_With_Tenant_Detail()
    {
        var tenantId = $"tenant-{Guid.NewGuid()}";
        await fixture.SeedTenantPolicyAsync(tenantId, new CachedRateLimitPolicy
        {
            HasPolicy = true,
            Rules =
            [
                new CachedRateLimitRule { Domain = "*", Scope = RateLimitScopeNames.Tenant, Unit = "Minute", RequestsPerUnit = 1 },
                new CachedRateLimitRule { Domain = "*", Scope = RateLimitScopeNames.User, Unit = "Minute", RequestsPerUnit = 100 }
            ]
        });

        var first = await SendRequestAsync(tenantId, "user-a");
        var second = await SendRequestAsync(tenantId, "user-b");

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        second.Headers.RetryAfter.Should().NotBeNull();

        var detail = await GetErrorDetailAsync(second);
        detail.Should().Contain("tenant");
    }

    [Fact]
    public async Task User_Limit_Exceeded_Returns_429_With_User_Detail()
    {
        var tenantId = $"tenant-{Guid.NewGuid()}";
        await fixture.SeedTenantPolicyAsync(tenantId, new CachedRateLimitPolicy
        {
            HasPolicy = true,
            Rules =
            [
                new CachedRateLimitRule { Domain = "*", Scope = RateLimitScopeNames.Tenant, Unit = "Minute", RequestsPerUnit = 100 },
                new CachedRateLimitRule { Domain = "*", Scope = RateLimitScopeNames.User, Unit = "Minute", RequestsPerUnit = 1 }
            ]
        });

        var first = await SendRequestAsync(tenantId, "user-a");
        var second = await SendRequestAsync(tenantId, "user-a");

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        var detail = await GetErrorDetailAsync(second);
        detail.Should().Contain("request rate limit");
    }

    [Fact]
    public async Task Admitted_Request_Carries_RateLimit_Headers()
    {
        var tenantId = $"tenant-{Guid.NewGuid()}";
        await fixture.SeedTenantPolicyAsync(tenantId, new CachedRateLimitPolicy
        {
            HasPolicy = true,
            Rules =
            [
                new CachedRateLimitRule { Domain = "*", Scope = RateLimitScopeNames.Tenant, Unit = "Minute", RequestsPerUnit = 10 },
                new CachedRateLimitRule { Domain = "*", Scope = RateLimitScopeNames.User, Unit = "Minute", RequestsPerUnit = 10 }
            ]
        });

        var response = await SendRequestAsync(tenantId, "user-a");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("RateLimit-Limit");
        response.Headers.Should().ContainKey("RateLimit-Remaining");
        response.Headers.Should().ContainKey("RateLimit-Reset");
    }

    private async Task<HttpResponseMessage> SendRequestAsync(string tenantId, string userId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/test");
        request.Headers.Add(TestUserDetailsProvider.TenantIdHeader, tenantId);
        request.Headers.Add(TestUserDetailsProvider.UserIdHeader, userId);

        return await fixture.Client.SendAsync(request);
    }

    private static async Task<string?> GetErrorDetailAsync(HttpResponseMessage response)
    {
        var document = await response.Content.ReadFromJsonAsync<JsonDocument>();
        return document!.RootElement.GetProperty("errors")[0].GetProperty("detail").GetString();
    }
}
