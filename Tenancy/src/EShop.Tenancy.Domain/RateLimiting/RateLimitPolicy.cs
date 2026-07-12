namespace EShop.Tenancy.Domain.RateLimiting;

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
