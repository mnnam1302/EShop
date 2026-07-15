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

    public required string Scope { get; init; }

    public required string Unit { get; init; }

    public required int RequestsPerUnit { get; init; }

    public int? Burst { get; init; }
}
