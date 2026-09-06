using GameBackend.Services.Catalog.API.Application.Commands;
using GameBackend.Services.Catalog.API.Application.Interfaces;
using GameBackend.Services.Catalog.API.Application.Queries;
using GameBackend.Services.Catalog.API.Infrastructure.Persistence;
using GameBackend.Services.Catalog.API.Infrastructure.Persistence.Repositories;
using GameBackend.SharedKernel.Security;
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

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CatalogDb"),
        npgsql => npgsql.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null)));

builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<CreateItemCommandHandler>();
builder.Services.AddScoped<GetItemsQueryHandler>();
builder.Services.AddScoped<GetInventoryQueryHandler>();
builder.Services.AddScoped<BuyItemCommandHandler>();
builder.Services.AddScoped<GrantItemCommandHandler>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();

var internalApiKey = builder.Configuration["InternalApi:Key"];
if (string.IsNullOrWhiteSpace(internalApiKey))
    throw new InvalidOperationException("InternalApi:Key не задан — установите переменную окружения InternalApi__Key.");

builder.Services.AddSingleton<IInternalCallerValidator>(new InternalCallerValidator(internalApiKey));

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
if (jwtSettings is not null)
{
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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await CatalogDbInitializer.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();