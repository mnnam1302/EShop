namespace EShop.Shared.Contracts.Shared;

public class ActionDefinition
{
    public ActionDefinition(bool isAllowed)
    {
        IsAllowed = isAllowed;
    }

    public bool IsAllowed { get; init; }
}