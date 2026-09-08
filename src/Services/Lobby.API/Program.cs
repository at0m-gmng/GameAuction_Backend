using GameBackend.Services.Lobby.API.Application.Commands;
using GameBackend.Services.Lobby.API.Application.Interfaces;
using GameBackend.Services.Lobby.API.Application.Queries;
using GameBackend.Services.Lobby.API.Application.Services;
using GameBackend.Services.Lobby.API.Hubs;
using GameBackend.Services.Lobby.API.Infrastructure.Configuration;
using GameBackend.Services.Lobby.API.Infrastructure.Events;
using GameBackend.Services.Lobby.API.Infrastructure.ExternalServices;
using GameBackend.Services.Lobby.API.Infrastructure.Persistence;
using GameBackend.Services.Lobby.API.Infrastructure.Persistence.Repositories;
using GameBackend.SharedKernel.Application;
using GameBackend.SharedKernel.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

const string FrontendCorsPolicy = "Frontend";
const int HmacSha256MinKeyBytes = 32;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins("https://at0m-gmng.github.io")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddDbContext<LobbyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("LobbyDb"),
        npgsql => npgsql.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null)));

builder.Services.AddScoped<ILobbyRepository, LobbyRepository>();
builder.Services.AddScoped<IDomainEventDispatcher, SignalRDomainEventDispatcher>();

builder.Services.AddScoped<CreateLobbyCommandHandler>();
builder.Services.AddScoped<JoinLobbyCommandHandler>();
builder.Services.AddScoped<LeaveLobbyCommandHandler>();
builder.Services.AddScoped<PlaceBidCommandHandler>();
builder.Services.AddScoped<AuctionCompletionService>();
builder.Services.AddScoped<GetOpenLobbiesQueryHandler>();
builder.Services.AddScoped<GetLobbyQueryHandler>();

var internalApi = builder.Configuration.GetSection(InternalApiSettings.SectionName).Get<InternalApiSettings>()
    ?? throw new InvalidOperationException($"Конфигурация '{InternalApiSettings.SectionName}' отсутствует.");

if (string.IsNullOrWhiteSpace(internalApi.Key))
    throw new InvalidOperationException("InternalApi:Key не задан — установите переменную окружения InternalApi__Key.");
if (string.IsNullOrWhiteSpace(internalApi.IdentityBaseUrl))
    throw new InvalidOperationException("InternalApi:IdentityBaseUrl не задан.");
if (string.IsNullOrWhiteSpace(internalApi.CatalogBaseUrl))
    throw new InvalidOperationException("InternalApi:CatalogBaseUrl не задан.");

// NOTE: тот же ключ — и для исходящих вызовов (заголовок ниже), и для входящих проверок.
builder.Services.AddSingleton<IInternalCallerValidator>(new InternalCallerValidator(internalApi.Key));

// NOTE: короткий таймаут — расчёт по завершённому аукциону best-effort, не должен подвешивать запрос.
var internalApiTimeout = TimeSpan.FromSeconds(5);

builder.Services.AddHttpClient<IIdentityServiceClient, IdentityServiceClient>(client =>
{
    client.BaseAddress = new Uri(internalApi.IdentityBaseUrl);
    client.Timeout = internalApiTimeout;
    client.DefaultRequestHeaders.Add("X-Internal-Key", internalApi.Key);
});

builder.Services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(client =>
{
    client.BaseAddress = new Uri(internalApi.CatalogBaseUrl);
    client.Timeout = internalApiTimeout;
    client.DefaultRequestHeaders.Add("X-Internal-Key", internalApi.Key);
});

// NOTE: Issuer/Audience/SecretKey должны совпадать с Identity.API — токены подписывает он.
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        $"Конфигурация JWT отсутствует: секция '{JwtSettings.SectionName}' не найдена.");

if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
    throw new InvalidOperationException("Jwt:Issuer не задан.");
if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
    throw new InvalidOperationException("Jwt:Audience не задан.");
if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
    throw new InvalidOperationException("Jwt:SecretKey не задан — установите переменную окружения Jwt__SecretKey.");
if (Encoding.UTF8.GetByteCount(jwtSettings.SecretKey) < HmacSha256MinKeyBytes)
    throw new InvalidOperationException($"Jwt:SecretKey слишком короткий для HS256: нужно минимум {HmacSha256MinKeyBytes} байт.");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // NOTE: без этого JwtBearerHandler молча переименовывает claim "sub" в легаси ClaimTypes.NameIdentifier.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<LobbyHub>("/hubs/lobby");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LobbyDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();