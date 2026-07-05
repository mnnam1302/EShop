using FluentAssertions;

namespace EShop.Finance.Tests.Application.Integrations.TemplateData;

public sealed class TemplateDataModelBaseTests
{
    [Fact]
    public void Formats_date_using_short_date_format()
    {
        var model = LeafModel.Parse("yyyy-MM-dd", new DateOnly(2026, 3, 7));

        model.GetTemplateDataModel()["dueDate"].Should().Be("2026-03-07");
    }

    [Fact]
    public void Nests_child_model_under_its_key()
    {
        var model = ParentModel.Parse();

        var nested = model.GetTemplateDataModel()["child"].Should().BeAssignableTo<IReadOnlyDictionary<string, object?>>().Subject;
        nested["name"].Should().Be("child-1");
    }

    [Fact]
    public void Flattens_child_model_without_prefix()
    {
        var model = ParentModel.Parse();

        model.GetTemplateDataModel()["name"].Should().Be("flat-1");
    }

    [Fact]
    public void Prefixes_dictionary_entries_with_the_property_key()
    {
        var model = ParentModel.Parse();

        model.GetTemplateDataModel()["extras.foo"].Should().Be("bar");
    }

    [Fact]
    public void Collects_sensitive_keys_including_nested_prefixes()
    {
        var model = ParentModel.Parse();

        model.GetSensitiveKeys().Should().Contain(["secret", "child.name"]);
    }
}
