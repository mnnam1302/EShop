using EShop.Finance.API;
using EShop.Finance.Application.DependencyInjection;
using EShop.Finance.Infrastructure;
using EShop.Finance.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddFinanceApplication()
    .AddFinancePersistence(builder.Configuration, builder.Environment)
    .AddFinanceInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

// Apply pending migrations on startup (matches the other EShop services' local-dev approach).
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

FinanceEndpoints.Map(app);

app.Run();
