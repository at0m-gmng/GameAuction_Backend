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
if (jwtSettings is not null)
{
    builder.Services.AddSingleton(jwtSettings);

    builder.Services.AddAuthentication(options =>
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
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