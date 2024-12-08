using EShop.Identity.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EShop.Identity.API.DependencyInjections.Extensions;

public static class MigrationExtensions
{
    public static async void ApplyMigrations(this IApplicationBuilder app)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();

            await dbInitializer.Initialize();
        }
    }
}