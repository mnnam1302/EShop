namespace EShop.Shared.Sequences;

public class TenantSequence
{
    public string Application { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public int SeedValue { get; set; }

    public string GetSequenceId(string sequenceType) => GetTenantSequenceId(sequenceType, this.TenantId.ToLowerInvariant());

    public static string GetTenantSequenceId(string sequenceType, string tenantId) => $"{sequenceType}_{tenantId}";
}
