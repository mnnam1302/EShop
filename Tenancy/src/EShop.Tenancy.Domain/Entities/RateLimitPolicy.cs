using EShop.Tenancy.Domain.Enumerations;

namespace EShop.Tenancy.Domain.Entities;

public class RateLimitPolicy
{
    private readonly List<RateLimitRule> _rules = [];

    public RateLimitPolicy()
    {
    }

    public RateLimitPolicy(IEnumerable<RateLimitRule> rules)
    {
        _rules = rules.ToList();
    }

    public IReadOnlyCollection<RateLimitRule> Rules => _rules.AsReadOnly();
}

public class RateLimitRule
{
    public const string AllDomains = "*";

    public required string Domain { get; init; }

    public required RateLimitScope Scope { get; init; }

    public required RateLimitUnit Unit { get; init; }

    public required int RequestsPerUnit { get; init; }

    public int? Burst { get; init; }
}
