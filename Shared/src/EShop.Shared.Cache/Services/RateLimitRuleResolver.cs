namespace EShop.Shared.Cache.Services;

public interface IRateLimitRuleResolver
{
    CachedRateLimitRule ResolveRule(CachedRateLimitPolicy? tenantPolicy, CachedRateLimitPolicy? systemPolicy, string domain, string scope);
}

public sealed class RateLimitRuleResolver : IRateLimitRuleResolver
{
    public CachedRateLimitRule ResolveRule(CachedRateLimitPolicy? tenantPolicy, CachedRateLimitPolicy? systemPolicy, string domain, string scope)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);

        return FindRule(tenantPolicy, domain, scope)
            ?? FindRule(tenantPolicy, CachedRateLimitRule.AllDomains, scope)
            ?? FindRule(systemPolicy, domain, scope)
            ?? FindRule(systemPolicy, CachedRateLimitRule.AllDomains, scope)
            ?? CompiledSafetyDefaults.GetDefaultRule(scope);
    }

    private static CachedRateLimitRule? FindRule(CachedRateLimitPolicy? policy, string domain, string scope)
    {
        if (policy is null || !policy.HasPolicy)
        {
            return null;
        }

        return policy.Rules.FirstOrDefault(rule =>
            string.Equals(rule.Domain, domain, StringComparison.Ordinal) &&
            string.Equals(rule.Scope, scope, StringComparison.Ordinal));
    }
}
