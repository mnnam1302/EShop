namespace EShop.Finance.Application.Services.IntegrationProvider.TemplateData;

public interface ITemplateDataModel
{
    IReadOnlyDictionary<string, object?> GetTemplateDataModel();

    IReadOnlyCollection<string> GetSensitiveKeys();
}
