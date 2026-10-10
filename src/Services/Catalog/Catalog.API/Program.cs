using GameBackend.ServiceDefaults.Extensions;
using GameBackend.Services.Catalog.API.Validation;
using GameBackend.Services.Catalog.Application;
using GameBackend.Services.Catalog.Infrastructure;
using GameBackend.Services.Catalog.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceLogging();

builder.Services
    .AddApiDocumentation()
    .AddValidatedControllers<BuyItemRequestValidator>()
    .AddFrontendCors(allowCredentials: false)
    .AddGameJwtAuthentication()
    .AddCatalogApplication()
    .AddCatalogInfrastructure(builder.Configuration);

WebApplication app = builder.Build();

app.UseRequestObservability();

await app.Services.InitializeCatalogDatabaseAsync();

app.MapApiDocumentation();

app.UseHttpsRedirection();
app.UseCors(HostingExtensions.FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks(HostingExtensions.HealthPath);

app.Run();
