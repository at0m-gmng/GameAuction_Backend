using GameBackend.ServiceDefaults.Extensions;
using GameBackend.Services.Generation.Application;
using GameBackend.Services.Generation.Infrastructure;
using GameBackend.Services.Generation.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceLogging();

builder.Services.AddControllers();
builder.Services
    .AddGenerationApplication()
    .AddGenerationInfrastructure(builder.Configuration);

WebApplication app = builder.Build();

app.UseRequestObservability();

await app.Services.InitializeGenerationDatabaseAsync();

app.MapControllers();
app.MapHealthChecks(HostingExtensions.HealthPath);

app.Run();
