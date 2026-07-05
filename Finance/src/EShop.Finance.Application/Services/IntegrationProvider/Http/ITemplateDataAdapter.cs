namespace EShop.Finance.Application.Services.IntegrationProvider.Http;

public interface ITemplateDataAdapter<in TModel>
{
    IReadOnlyDictionary<string, object?> ToTemplateData(TModel model, TemplateRenderContext context);
}
