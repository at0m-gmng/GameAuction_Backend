using GameBackend.Services.Identity.API.Application.Commands;
using GameBackend.Services.Identity.API.Application.Interfaces;
using GameBackend.Services.Identity.API.Infrastructure.Persistence;
using GameBackend.Services.Identity.API.Infrastructure.Persistence.Repositories;
using GameBackend.Services.Identity.API.Infrastructure.Security;
using GameBackend.SharedKernel.Security;
using GameBackend.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

const string FrontendCorsPolicy = "Frontend";

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
builder.Services.AddSingleton<JwtTokenGenerator>();

builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();

builder.Services.AddScoped<ICommandHandler<RegisterCommand, string>, RegisterCommandHandler>();
builder.Services.AddScoped<ICommandHandler<LoginCommand, string>, LoginCommandHandler>();

// Bound exactly once and reused as the same instance for both token
// generation (JwtTokenGenerator, via DI below) and validation (below) —
// previously these were two independent bindings (Configure<JwtSettings> +
// IOptions vs a raw .Get<JwtSettings>() call) that could silently diverge,
// which is exactly what happened: generation saw a populated Audience,
// validation saw an empty one, and every token was rejected as a result.
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();

// TEMPORARY DIAGNOSTICS for the IDX10208 investigation — remove once the
// root cause is confirmed. Printed with Console.WriteLine (not ILogger,
// since no DI/logging pipeline exists yet at this point) so it's guaranteed
// to show up in Render's stdout log stream. Prefixed "JWT-DIAG" for easy
// filtering via list_logs text search.
Console.WriteLine(jwtSettings is null
    ? "JWT-DIAG startup: GetSection(\"Jwt\").Get<JwtSettings>() returned NULL — the Jwt config section is missing entirely."
    : $"JWT-DIAG startup: Environment={builder.Environment.EnvironmentName} Issuer='{jwtSettings.Issuer}' Audience='{jwtSettings.Audience}' SecretKeyLen={jwtSettings.SecretKey?.Length ?? -1} ExpiryMinutes={jwtSettings.ExpiryMinutes}");

if (jwtSettings is not null)
{
    // THE ACTUAL ROOT CAUSE of the IDX10208 loop: the token generator falls
    // back to these defaults when config arrives empty (see JwtTokenGenerator),
    // but validation below read jwtSettings.Audience raw. So when Audience was
    // empty at runtime, generation still stamped a correct "game-backend-clients"
    // aud onto the token (via its fallback) while validation set ValidAudience
    // to "" — hence a perfect-looking token rejected with "ValidAudience is null
    // or whitespace". Applying the same fallbacks here, once, makes the two sides
    // physically incapable of disagreeing. The generator's own fallback then
    // becomes redundant but harmless.
    jwtSettings.Issuer = string.IsNullOrWhiteSpace(jwtSettings.Issuer) ? "game-backend" : jwtSettings.Issuer;
    jwtSettings.Audience = string.IsNullOrWhiteSpace(jwtSettings.Audience) ? "game-backend-clients" : jwtSettings.Audience;

    // SecretKey has no safe default — a signing key can't be guessed. Fail loudly
    // at startup rather than hitting a NullReference deep inside GetBytes on the
    // first request (this also resolves the CS8604 nullable warning cleanly).
    if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
        throw new InvalidOperationException("Jwt:SecretKey is not configured — set Jwt__SecretKey on the deployed environment.");

    Console.WriteLine($"JWT-DIAG after-normalize: Issuer='{jwtSettings.Issuer}' Audience='{jwtSettings.Audience}' SecretKeyLen={jwtSettings.SecretKey.Length}");

    // Captured into a local after the guard above so nullable flow analysis knows
    // it's non-null (the property-access form left a CS8604 warning at GetBytes).
    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

    builder.Services.AddSingleton(jwtSettings);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Audience = jwtSettings.Audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };

        // Confirms this closure actually ran (and with what values) — if this
        // line never appears in logs, IOptionsFactory never invoked our
        // configure delegate at all, which would point at a completely
        // different problem than a bad Issuer/Audience value.
        Console.WriteLine($"JWT-DIAG AddJwtBearer configure ran: ValidIssuer='{options.TokenValidationParameters.ValidIssuer}' ValidAudience='{options.TokenValidationParameters.ValidAudience}' Options.Audience='{options.Audience}'");

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                Console.WriteLine($"JWT-DIAG OnMessageReceived: hasToken={!string.IsNullOrEmpty(context.Token)} len={context.Token?.Length ?? 0}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"JWT-DIAG OnTokenValidated: sub={context.Principal?.FindFirst("sub")?.Value}");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                var tvp = context.Options.TokenValidationParameters;
                Console.WriteLine(
                    $"JWT-DIAG OnAuthenticationFailed: exception={context.Exception.GetType().FullName} message=\"{context.Exception.Message}\" " +
                    $"live.ValidIssuer='{tvp.ValidIssuer}' live.ValidAudience='{tvp.ValidAudience}' " +
                    $"live.ValidIssuersCount={tvp.ValidIssuers?.Count() ?? -1} live.ValidAudiencesCount={tvp.ValidAudiences?.Count() ?? -1} " +
                    $"live.Options.Audience='{context.Options.Audience}'");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"JWT-DIAG OnChallenge: error='{context.Error}' description='{context.ErrorDescription}' authFailureMessage='{context.AuthenticateFailure?.Message}'");
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();
}

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
}

app.Run();

public partial class Program { }