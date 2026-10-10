using GameBackend.ServiceDefaults.Extensions;
using GameBackend.Services.Identity.API.Extensions;
using GameBackend.Services.Identity.API.Validation;
using GameBackend.Services.Identity.Application;
using GameBackend.Services.Identity.Infrastructure;
using GameBackend.Services.Identity.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.UsePlatformPort().AddServiceLogging();

builder.Services
    .AddApiDocumentation()
    .AddValidatedControllers<RegisterRequestValidator>()
    .AddFrontendCors(allowCredentials: false)
    .AddGameJwtAuthentication()
    .AddAuthRateLimiting()
    .AddIdentityApplication()
    .AddIdentityInfrastructure(builder.Configuration);

WebApplication app = builder.Build();

app.UseRequestObservability();
app.MapApiDocumentation();

app.UseHttpsRedirection();
app.UseCors(HostingExtensions.FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.MapHealthChecks(HostingExtensions.HealthPath);

await app.Services.InitializeIdentityDatabaseAsync();

app.Run();

/// <summary>
/// Точка входа; открыта для WebApplicationFactory в интеграционных тестах.
/// </summary>
public partial class Program
{
}
