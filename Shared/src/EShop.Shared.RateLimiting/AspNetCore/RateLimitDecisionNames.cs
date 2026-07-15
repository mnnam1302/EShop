namespace EShop.Shared.RateLimiting.AspNetCore;

internal static class RateLimitDecisionNames
{
    public static string From(bool allowed, bool enforced)
    {
        if (allowed)
        {
            return "allow";
        }

        return enforced ? "reject" : "shadow_reject";
    }
}
