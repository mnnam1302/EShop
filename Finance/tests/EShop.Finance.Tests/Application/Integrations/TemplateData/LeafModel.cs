using EShop.Finance.Application.Services.IntegrationProvider.TemplateData;

namespace EShop.Finance.Tests.Application.Integrations.TemplateData;

public sealed class LeafModel : TemplateDataModelBase
{
    [TemplateData]
    public DateOnly DueDate { get; private init; }

    public static LeafModel Parse(string dateFormat, DateOnly dueDate)
    {
        var model = new LeafModel { ShortDateFormat = dateFormat, DueDate = dueDate };
        model.BuildTemplateData();
        return model;
    }
}
