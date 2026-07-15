using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;

namespace EShop.Shared.JsonApi.Tests.RateLimiting;

// RateLimitPolicyHttpClient (a constructor dependency of RateLimitPolicyResolver) requires this
// factory to exist, even though these tests never trigger the HTTP fallback path (Redis/L1 are
// always pre-seeded). Its method is never actually called here.
internal sealed class FakeSystemInternalJwtTokenFactory : ISystemInternalJwtTokenFactory
{
    public Task<HttpClient> AddUserContext(HttpClient client, UserData operationalUser, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Not used in tests — Redis/L1 cache is always pre-seeded.");
    }
}
