using GameBackend.Services.Identity.API.Application.Commands;
using GameBackend.Services.Identity.API.Application.Interfaces;
using GameBackend.Services.Identity.API.Application.Services;
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
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<ICommandHandler<RegisterCommand, string>, RegisterCommandHandler>();
builder.Services.AddScoped<ICommandHandler<LoginCommand, string>, LoginCommandHandler>();

var internalApiKey = builder.Configuration["InternalApi:Key"];
if (string.IsNullOrWhiteSpace(internalApiKey))
    throw new InvalidOperationException("InternalApi:Key не задан — установите переменную окружения InternalApi__Key.");

var generationBaseUrl = builder.Configuration["InternalApi:GenerationBaseUrl"];
if (string.IsNullOrWhiteSpace(generationBaseUrl))
    throw new InvalidOperationException("InternalApi:GenerationBaseUrl не задан.");

var catalogBaseUrl = builder.Configuration["InternalApi:CatalogBaseUrl"];
if (string.IsNullOrWhiteSpace(catalogBaseUrl))
    throw new InvalidOperationException("InternalApi:CatalogBaseUrl не задан.");

// NOTE: короткий таймаут — недоступность Generation/Catalog.API не должна задерживать вход.
var internalApiTimeout = TimeSpan.FromSeconds(5);

builder.Services.AddHttpClient<IGenerationServiceClient, GenerationServiceClient>(client =>
{
    client.BaseAddress = new Uri(generationBaseUrl);
    client.Timeout = internalApiTimeout;
    client.DefaultRequestHeaders.Add("X-Internal-Key", internalApiKey);
});

builder.Services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(client =>
{
    client.BaseAddress = new Uri(catalogBaseUrl);
    client.Timeout = internalApiTimeout;
    client.DefaultRequestHeaders.Add("X-Internal-Key", internalApiKey);
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
        db.Database.ExecuteSqlRaw(
            """ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "WelcomeGiftGranted" boolean NOT NULL DEFAULT false;""");
}

app.Run();

// NOTE: exposed for WebApplicationFactory<Program> in integration tests.
public partial class Program { }
