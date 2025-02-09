using Ardalis.SmartEnum;

namespace EShop.Tenancy.Domain.Enumerations;

public class StateFeature : SmartEnum<StateFeature>
{
    public StateFeature(string name, int value)
        : base(name, value)
    {
    }

    public static readonly StateFeature Enabled = new StateFeature(nameof(Enabled), 1);
    public static readonly StateFeature Disabled = new StateFeature(nameof(Disabled), 2);

    public static StateFeature FromValue(int value) => FromValue(value);
    public static StateFeature FromName(string name) => FromName(name);

    public static implicit operator int(StateFeature state) => state.Value;
    public static implicit operator string(StateFeature state) => state.Name;
}