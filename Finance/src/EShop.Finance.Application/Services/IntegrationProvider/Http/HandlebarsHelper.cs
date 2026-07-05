using HandlebarsDotNet;

namespace EShop.Finance.Application.Services.IntegrationProvider.Http;

/// <summary>
/// Compiles and renders Handlebars templates for URLs, request bodies, and response shaping.
/// </summary>
public static class HandlebarsHelper
{
    private static readonly IHandlebars Engine = Handlebars.Create();

    public static string Render(string template, object data)
    {
        var compiled = Engine.Compile(template);
        return compiled(data);
    }
}
