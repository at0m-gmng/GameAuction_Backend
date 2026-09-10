using GameBackend.Services.Identity.API.Application.Commands;
using GameBackend.Services.Identity.API.Application.Interfaces;
using GameBackend.Services.Identity.API.Application.Queries;
using GameBackend.Services.Identity.API.Application.Services;
using GameBackend.Services.Identity.API.Infrastructure.Configuration;
using GameBackend.Services.Identity.API.Infrastructure.ExternalServices;
using GameBackend.Services.Identity.API.Infrastructure.Persistence;
using GameBackend.Services.Identity.API.Infrastructure.Persistence.Repositories;
using GameBackend.Services.Identity.API.Infrastructure.Security;
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

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins("https://at0m-gmng.github.io")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("IdentityDb"),
        npgsql => npgsql.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null)));

builder.Services.AddSingleton<PasswordHasher>();

// NOTE: не fail-fast — это игровой параметр, а не секрет; отсутствие секции даёт StartingBalance = 0.
var economySettings = builder.Configuration.GetSection(EconomySettings.SectionName).Get<EconomySettings>()
    ?? new EconomySettings();
builder.Services.AddSingleton(economySettings);
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<ICommandHandler<RegisterCommand, string>, RegisterCommandHandler>();
builder.Services.AddScoped<ICommandHandler<LoginCommand, string>, LoginCommandHandler>();

var internalApi = builder.Configuration.GetSection(InternalApiSettings.SectionName).Get<InternalApiSettings>()
    ?? throw new InvalidOperationException($"Конфигурация '{InternalApiSettings.SectionName}' отсутствует.");

if (string.IsNullOrWhiteSpace(internalApi.Key))
    throw new InvalidOperationException("InternalApi:Key не задан — установите переменную окружения InternalApi__Key.");
if (string.IsNullOrWhiteSpace(internalApi.GenerationBaseUrl))
    throw new InvalidOperationException("InternalApi:GenerationBaseUrl не задан.");
if (string.IsNullOrWhiteSpace(internalApi.CatalogBaseUrl))
    throw new InvalidOperationException("InternalApi:CatalogBaseUrl не задан.");

// NOTE: тот же ключ и на вход (Lobby.API дёргает /internal/debit), и на выход (звонки в Catalog/Generation).
builder.Services.AddSingleton<IInternalCallerValidator>(new InternalCallerValidator(internalApi.Key));
builder.Services.AddScoped<DebitBalanceCommandHandler>();
builder.Services.AddScoped<CreditBalanceCommandHandler>();
builder.Services.AddScoped<GetPlayerBalanceQueryHandler>();

// NOTE: короткий таймаут — недоступность Generation/Catalog.API не должна задерживать вход.
var internalApiTimeout = TimeSpan.FromSeconds(5);

builder.Services.AddHttpClient<IGenerationServiceClient, GenerationServiceClient>(client =>
{
    client.BaseAddress = new Uri(internalApi.GenerationBaseUrl);
    client.Timeout = internalApiTimeout;
    client.DefaultRequestHeaders.Add("X-Internal-Key", internalApi.Key);
});

builder.Services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(client =>
{
    client.BaseAddress = new Uri(internalApi.CatalogBaseUrl);
    client.Timeout = internalApiTimeout;
    client.DefaultRequestHeaders.Add("X-Internal-Key", internalApi.Key);
});

builder.Services.AddScoped<WelcomeGiftFulfiller>();

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

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<JwtTokenGenerator>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    db.Database.EnsureCreated();

    // NOTE: EnsureCreated не доливает колонки в старую БД — патч идемпотентен, только для Postgres.
    if (db.Database.IsNpgsql())
    {
        db.Database.ExecuteSqlRaw(
            """ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "WelcomeGiftGranted" boolean NOT NULL DEFAULT false;""");

        // NOTE: DEFAULT false — бэкфилл; старые игроки дополучат баланс при следующем логине.
        db.Database.ExecuteSqlRaw(
            """ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "StartingBalanceGranted" boolean NOT NULL DEFAULT false;""");

        // NOTE: "Inventory" — осиротевшая колонка от удалённого неиспользуемого Player._inventory.
        db.Database.ExecuteSqlRaw("""ALTER TABLE "Players" DROP COLUMN IF EXISTS "Inventory";""");
    }
}

app.Run();

// NOTE: exposed for WebApplicationFactory<Program> in integration tests.
public partial class Program { }
