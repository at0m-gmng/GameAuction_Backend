using GameBackend.Services.Catalog.API.Application.Commands;
using GameBackend.Services.Catalog.API.Application.Interfaces;
using GameBackend.Services.Catalog.API.Application.Queries;
using GameBackend.Services.Catalog.API.Infrastructure.Configuration;
using GameBackend.Services.Catalog.API.Infrastructure.ExternalServices;
using GameBackend.Services.Catalog.API.Infrastructure.Observability;
using GameBackend.Services.Catalog.API.Infrastructure.Persistence;
using GameBackend.Services.Catalog.API.Infrastructure.Persistence.Repositories;
using GameBackend.Services.Catalog.API.Infrastructure.Validation;
using GameBackend.SharedKernel.Security;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Text;

const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(new RenderedCompactJsonFormatter()));

builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<BuyItemRequestValidator>();
builder.Services.AddControllers(options => options.Filters.Add<ValidationActionFilter>());

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins("https://at0m-gmng.github.io")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CatalogDb"),
        npgsql => npgsql.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null)));

builder.Services.AddHealthChecks().AddDbContextCheck<CatalogDbContext>();

builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<CreateItemCommandHandler>();
builder.Services.AddScoped<GetItemsQueryHandler>();
builder.Services.AddScoped<GetInventoryQueryHandler>();
builder.Services.AddScoped<GetMyListingsQueryHandler>();
builder.Services.AddScoped<BuyItemCommandHandler>();
builder.Services.AddScoped<AwardItemCommandHandler>();
builder.Services.AddScoped<GrantItemCommandHandler>();
builder.Services.AddScoped<ListInventoryItemForSaleCommandHandler>();
builder.Services.AddScoped<UnlistItemCommandHandler>();
builder.Services.AddScoped<StartAuctionCommandHandler>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();

var internalApi = builder.Configuration.GetSection(InternalApiSettings.SectionName).Get<InternalApiSettings>()
    ?? throw new InvalidOperationException($"Конфигурация '{InternalApiSettings.SectionName}' отсутствует.");

if (string.IsNullOrWhiteSpace(internalApi.Key))
    throw new InvalidOperationException("InternalApi:Key не задан — установите переменную окружения InternalApi__Key.");
if (string.IsNullOrWhiteSpace(internalApi.GenerationBaseUrl))
    throw new InvalidOperationException("InternalApi:GenerationBaseUrl не задан.");
if (string.IsNullOrWhiteSpace(internalApi.LobbyBaseUrl))
    throw new InvalidOperationException("InternalApi:LobbyBaseUrl не задан.");
if (string.IsNullOrWhiteSpace(internalApi.IdentityBaseUrl))
    throw new InvalidOperationException("InternalApi:IdentityBaseUrl не задан.");

builder.Services.AddSingleton<IInternalCallerValidator>(new InternalCallerValidator(internalApi.Key));
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdHandler>();

builder.Services.AddHttpClient<IGenerationServiceClient, GenerationServiceClient>(client =>
{
    client.BaseAddress = new Uri(internalApi.GenerationBaseUrl);
    client.DefaultRequestHeaders.Add("X-Internal-Key", internalApi.Key);
})
    .AddHttpMessageHandler<CorrelationIdHandler>()
    // NOTE: retry будит уснувший Generation.API; длинный attempt-timeout под холодный старт free-tier.
    .AddStandardResilienceHandler(o =>
    {
        o.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
        o.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(90);
        o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);
    });

builder.Services.AddHttpClient<ILobbyServiceClient, LobbyServiceClient>(client =>
{
    client.BaseAddress = new Uri(internalApi.LobbyBaseUrl);
    client.DefaultRequestHeaders.Add("X-Internal-Key", internalApi.Key);
})
    .AddHttpMessageHandler<CorrelationIdHandler>()
    .AddStandardResilienceHandler();

builder.Services.AddHttpClient<IIdentityServiceClient, IdentityServiceClient>(client =>
{
    client.BaseAddress = new Uri(internalApi.IdentityBaseUrl);
    client.DefaultRequestHeaders.Add("X-Internal-Key", internalApi.Key);
})
    .AddHttpMessageHandler<CorrelationIdHandler>()
    // NOTE: покупка-debit не идемпотентна (без ключа) — retry запрещён, только timeout + circuit breaker.
    .AddStandardResilienceHandler(o => o.Retry.MaxRetryAttempts = 0);

var marketplaceSettings = builder.Configuration.GetSection(MarketplaceSettings.SectionName).Get<MarketplaceSettings>()
    ?? new MarketplaceSettings();
builder.Services.AddSingleton(marketplaceSettings);
builder.Services.AddScoped<GeneratePublicItemCommandHandler>();

// NOTE: Issuer/Audience/SecretKey должны совпадать с Identity.API — токены подписывает он.
const int HmacSha256MinKeyBytes = 32;

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

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    var generatePublicItem = scope.ServiceProvider.GetRequiredService<GeneratePublicItemCommandHandler>();
    await CatalogDbInitializer.SeedAsync(db, generatePublicItem, marketplaceSettings, app.Logger);
}

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();