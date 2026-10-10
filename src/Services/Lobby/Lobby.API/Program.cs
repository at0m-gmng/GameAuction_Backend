using GameBackend.ServiceDefaults.Extensions;
using GameBackend.Services.Lobby.API.Extensions;
using GameBackend.Services.Lobby.API.Hubs;
using GameBackend.Services.Lobby.API.Validation;
using GameBackend.Services.Lobby.Application;
using GameBackend.Services.Lobby.Infrastructure;
using GameBackend.Services.Lobby.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.UsePlatformPort().AddServiceLogging();

builder.Services
    .AddApiDocumentation()
    .AddValidatedControllers<PlaceBidRequestValidator>()
    .AddFrontendCors(allowCredentials: true)
    .AddGameJwtAuthentication(RealtimeExtensions.ReadHubTokenFromQuery)
    .AddLobbyRealtime()
    .AddLobbyApplication()
    .AddLobbyInfrastructure(builder.Configuration);

WebApplication app = builder.Build();

app.UseRequestObservability();
app.MapApiDocumentation();

app.UseHttpsRedirection();
app.UseCors(HostingExtensions.FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<LobbyHub>(RealtimeExtensions.LobbyHubPath);
app.MapHealthChecks(HostingExtensions.HealthPath);

await app.Services.InitializeLobbyDatabaseAsync();

app.Run();
