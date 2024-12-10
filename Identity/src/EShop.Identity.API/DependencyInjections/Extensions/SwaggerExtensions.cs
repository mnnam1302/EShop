using Asp.Versioning.ApiExplorer;
using EShop.Identity.API.DependencyInjections.Options;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace EShop.Identity.API.DependencyInjections.Extensions;

public static class SwaggerExtensions
{
    public static void AddSwaggerAPI(this IServiceCollection services)
    {
        services.AddSwaggerGen();
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
    }

    public static void UseSwaggerAPI(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var version in provider.ApiVersionDescriptions.Select(version => version.GroupName))
            {
                options.SwaggerEndpoint($"/swagger/{version}/swagger.json", version);
            }

            options.DisplayRequestDuration();
            options.EnableTryItOutByDefault();
            options.DocExpansion(DocExpansion.None);
        });
    }

    // Old code using within Program
    //public static void UseSwaggerAPI(this WebApplication app)
    //{
    //    app.UseSwagger();
    //    app.UseSwaggerUI(options =>
    //    {
    //        foreach (var version in app.DescribeApiVersions().Select(version => version.GroupName))
    //        {
    //            options.SwaggerEndpoint($"/swagger/{version}/swagger.json", version);
    //        }

    //        options.DisplayRequestDuration();
    //        options.EnableTryItOutByDefault();
    //        options.DocExpansion(DocExpansion.None);
    //    });

    //    app.MapGet("/", () => Results.Redirect("/swagger/index.html"))
    //        .WithTags(string.Empty);
    //}
}