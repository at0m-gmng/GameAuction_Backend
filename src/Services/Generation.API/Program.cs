using GameBackend.Services.Generation.API.Application.Commands;
using GameBackend.Services.Generation.API.Application.Interfaces;
using GameBackend.Services.Generation.API.Domain;
using GameBackend.Services.Generation.API.Infrastructure.Persistence;
using GameBackend.Services.Generation.API.Infrastructure.Persistence.Repositories;
using GameBackend.SharedKernel.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<GenerationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("GenerationDb"),
        npgsql => npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)));

builder.Services.AddSingleton<IItemGenerator, ItemGenerator>();
builder.Services.AddScoped<IGenerationPoolRepository, GenerationPoolRepository>();
builder.Services.AddScoped<GenerateItemCommandHandler>();

var internalApiKey = builder.Configuration["InternalApi:Key"];
if (string.IsNullOrWhiteSpace(internalApiKey))
    throw new InvalidOperationException("InternalApi:Key не задан — установите переменную окружения InternalApi__Key.");

builder.Services.AddSingleton<IInternalCallerValidator>(new InternalCallerValidator(internalApiKey));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GenerationDbContext>();
    await GenerationDbInitializer.SeedAsync(db);
}

app.MapControllers();

app.Run();