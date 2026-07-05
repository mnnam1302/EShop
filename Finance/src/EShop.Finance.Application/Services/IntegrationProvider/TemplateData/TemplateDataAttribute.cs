namespace EShop.Finance.Application.Services.IntegrationProvider.TemplateData;

[AttributeUsage(AttributeTargets.Property)]
public sealed class TemplateDataAttribute : Attribute
{
    public string? PublicName { get; init; }

    public bool FormatDate { get; init; } = true;

    public bool FormatDateWithPrefix { get; init; }

    public bool Flatten { get; init; }
}
