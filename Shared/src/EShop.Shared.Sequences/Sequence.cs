using EShop.Shared.Scoping;

namespace EShop.Shared.Sequences;

public class Sequence : IExcludedFromScoping
{
    public string Id { get; set; } = string.Empty;
    public int NextAvailableValue { get; set; }
    public string ConcurrencyToken { get; set; } = DateTime.UtcNow.Ticks.ToString();

    public void UpdateAvailableValue(int reservedRange)
    {
        this.NextAvailableValue += reservedRange + 1;
        this.ConcurrencyToken = DateTime.UtcNow.Ticks.ToString();
    }
}