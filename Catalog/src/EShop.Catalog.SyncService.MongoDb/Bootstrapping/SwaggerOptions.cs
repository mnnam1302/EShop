using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EShop.Catalog.SyncService.MongoDb.Bootstrapping;

public class SwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public SwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new()
            {
                Title = AppDomain.CurrentDomain.FriendlyName,
                Version = description.ApiVersion.ToString()
            });
        }

        options.MapType<DateOnly>(() => new()
        {
            Format = "date",
            Example = new OpenApiString(DateOnly.MinValue.ToString())
        });

        options.CustomSchemaIds(type => type.ToString().Replace("+", "."));
    }
}
