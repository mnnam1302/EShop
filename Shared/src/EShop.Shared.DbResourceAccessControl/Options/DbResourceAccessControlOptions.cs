namespace EShop.Shared.DbResourceAccessControl.Options;

public sealed class DbResourceAccessControlOptions
{
    public const string SectionName = "DbResourceAccessControl";

    /// <summary>
    /// Indicates whether to adjust (i.e. ALTER POLICY) ring-fencing RLS policies on startup.
    /// </summary>
    public bool AdjustRingFencingRlsPoliciesOnStartUp { get; set; }

    /// <summary>
    /// Indicates whether to use the new ring-fencing RLS expression that filters against <c>root-org|*|customer-id</c>.
    /// </summary>
    public bool UseNewRingFencingRlsExpression { get; set; } = true;
}
