namespace EShop.Shared.RateLimiting.AspNetCore;

// D11 rollout: each layer ships in shadow mode (evaluate + count, never reject) until its flag is
// flipped to enforce via config, no redeploy. Defaulting every flag to false means an unconfigured
// gateway starts safely in shadow, matching the migration plan's "shadow first" step.
public sealed class RateLimiterEnforcementOptions
{
    public const string SectionName = "RateLimiting:Enforcement";

    public bool TenantEnforced { get; set; }
    public bool UserEnforced { get; set; }
    public bool AnonymousIpEnforced { get; set; }

    public bool IsEnforced(string layer) => layer switch
    {
        "tenant" => TenantEnforced,
        "user" => UserEnforced,
        "ip" => AnonymousIpEnforced,
        _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "Unknown rate-limit layer.")
    };
}
