using EShop.Finance.Application.Services.IntegrationProvider.TemplateData;

namespace EShop.Finance.Tests.Application.Integrations.TemplateData;

public sealed class ParentModel : TemplateDataModelBase
{
    [TemplateData(PublicName = "child")]
    [SensitiveData]
    public ChildModel? Child { get; private init; }

    [TemplateData(Flatten = true)]
    public ChildModel? FlattenedChild { get; private init; }

    [TemplateData(PublicName = "extras")]
    public IDictionary<string, string?>? Extras { get; private init; }

    [TemplateData(PublicName = "secret")]
    [SensitiveData]
    public string? ApiKey { get; private init; }

    public static ParentModel Parse()
    {
        var model = new ParentModel
        {
            Child = ChildModel.Parse("child-1"),
            FlattenedChild = FlattenedChildModel(),
            Extras = new Dictionary<string, string?> { ["foo"] = "bar" },
            ApiKey = "sensitive-value",
        };

        model.BuildTemplateData();
        return model;
    }

    private static ChildModel FlattenedChildModel()
    {
        var model = ChildModel.Parse("flat-1");
        return model;
    }
}
