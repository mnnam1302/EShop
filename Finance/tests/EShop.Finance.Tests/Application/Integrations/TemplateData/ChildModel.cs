using EShop.Finance.Application.Services.IntegrationProvider.TemplateData;

namespace EShop.Finance.Tests.Application.Integrations.TemplateData;

public sealed class ChildModel : TemplateDataModelBase
{
    [TemplateData]
    [SensitiveData]
    public string Name { get; private init; } = string.Empty;

    public static ChildModel Parse(string name)
    {
        var model = new ChildModel { Name = name };
        model.BuildTemplateData();
        return model;
    }
}
