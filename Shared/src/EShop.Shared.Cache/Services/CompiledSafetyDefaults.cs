namespace EShop.Shared.Cache.Services;

public static class CompiledSafetyDefaults
{
    private const string DefaultUnit = "Minute";

    public static CachedRateLimitRule GetDefaultRule(string scope)
    {
        return scope switch
        {
            RateLimitScopeNames.Tenant => new CachedRateLimitRule
            {
                Domain = CachedRateLimitRule.AllDomains,
                Scope = scope,
                Unit = DefaultUnit,
                RequestsPerUnit = 300,
                Burst = 300
            },
            RateLimitScopeNames.User => new CachedRateLimitRule
            {
                Domain = CachedRateLimitRule.AllDomains,
                Scope = scope,
                Unit = DefaultUnit,
                RequestsPerUnit = 60,
                Burst = 60
            },
            RateLimitScopeNames.AnonymousIp => new CachedRateLimitRule
            {
                Domain = CachedRateLimitRule.AllDomains,
                Scope = scope,
                Unit = DefaultUnit,
                RequestsPerUnit = 5,
                Burst = 5
            },
            _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unknown rate-limit scope.")
        };
    }
}
