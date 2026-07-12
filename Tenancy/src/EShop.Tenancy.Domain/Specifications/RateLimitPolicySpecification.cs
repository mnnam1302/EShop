using EShop.Shared.DomainTools.Specifications;
using EShop.Tenancy.Domain.Entities;
using EShop.Tenancy.Domain.Enumerations;

namespace EShop.Tenancy.Domain.Specifications;

public sealed class RateLimitPolicySpecification : Specification<RateLimitPolicy>
{
    public const int MaxRules = 20;

    private RateLimitPolicySpecification()
    {
    }

    public static RateLimitPolicySpecification New()
    {
        return new RateLimitPolicySpecification();
    }

    protected override IEnumerable<string> IsNotSatisfiedBecause(RateLimitPolicy policy)
    {
        if (policy is null)
        {
            yield return "Policy cannot be null.";
            yield break;
        }

        var rules = policy.Rules ?? [];

        if (rules.Count > MaxRules)
        {
            yield return $"policy contains {rules.Count} rules which exceeds the maximum of {MaxRules}";
        }

        var seenRules = new HashSet<(string Domain, RateLimitScope Scope)>(rules.Count);

        foreach (var rule in rules)
        {
            if (rule is null)
                continue;

            if (rule.RequestsPerUnit <= 0)
            {
                yield return $"rule '{rule.Domain}/{rule.Scope}' has a non-positive requestsPerUnit ({rule.RequestsPerUnit})";
            }

            if (rule.Burst is int burst && burst < rule.RequestsPerUnit)
            {
                yield return $"rule '{rule.Domain}/{rule.Scope}' has burst ({burst}) below requestsPerUnit ({rule.RequestsPerUnit})";
            }

            if (!seenRules.Add((rule.Domain, rule.Scope)))
            {
                yield return $"duplicate rule for domain '{rule.Domain}' and scope '{rule.Scope}'";
            }
        }
    }
}
