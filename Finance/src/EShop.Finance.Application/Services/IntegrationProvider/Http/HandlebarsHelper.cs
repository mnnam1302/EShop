using HandlebarsDotNet;

namespace EShop.Finance.Application.Services.IntegrationProvider.Http;

public static class HandlebarsHelper
{
    private static readonly IHandlebars Engine = Handlebars.Create();

    public static string Render(string template, object data)
    {
        var compiled = Engine.Compile(template);
        return compiled(data);
    }
}
