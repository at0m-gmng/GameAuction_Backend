using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GameBackend.Services.Identity.API.Controllers.Dtos;
using GameBackend.Services.Identity.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.API.Tests;

// End-to-end: register -> token -> [Authorize] /me, through the real Program.cs
// JWT wiring. Uses SQLite, not the EF InMemory provider — the latter mishandles
// the inventory PrimitiveCollection and 500s on register; SQLite stores it as
// JSON, close to how Npgsql stores uuid[].
public class AuthenticationIntegrationTests : WebApplicationFactory<Program>
{
    // appsettings.json ships only a 216-bit placeholder key; a valid one must come
    // via env var, because Program.cs reads config before app.Build(), after which
    // the factory's ConfigureAppConfiguration would be too late.
    private const string TestSecretKey = "integration-test-signing-key-that-is-long-enough-for-hs256";

    // A SQLite in-memory DB lives only while its connection is open, so keep it open
    // for the factory's lifetime — else EnsureCreated's schema is gone before requests.
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public AuthenticationIntegrationTests()
    {
        Environment.SetEnvironmentVariable("Jwt__SecretKey", TestSecretKey);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Development => developer exception page, so a 500 returns the real body.
        builder.UseEnvironment("Development");

        _connection.Open();

        builder.ConfigureServices(services =>
        {
            // AddDbContext registers several descriptors parameterised by
            // IdentityDbContext (options + the Npgsql provider config); strip them all
            // before re-adding SQLite, or EF sees two providers on one context.
            var identityDbDescriptors = services
                .Where(d => d.ServiceType.IsGenericType &&
                            d.ServiceType.GetGenericArguments().Contains(typeof(IdentityDbContext)))
                .ToList();
            foreach (var descriptor in identityDbDescriptors)
                services.Remove(descriptor);

            var contextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IdentityDbContext));
            if (contextDescriptor is not null)
                services.Remove(contextDescriptor);

            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
            // Env vars are process-global; clear it so it can't leak into other tests.
            Environment.SetEnvironmentVariable("Jwt__SecretKey", null);
        }
    }

    [Fact]
    public async Task Me_ReturnsUnauthorized_WithoutToken()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ReturnsOk_WithTokenFromFreshRegistration()
    {
        var client = CreateClient();
        var email = $"{Guid.NewGuid()}@test.local";

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("TestPlayer", email, "Password123!"));
        var registerBody = await registerResponse.Content.ReadAsStringAsync();
        Assert.True(registerResponse.StatusCode == HttpStatusCode.OK,
            $"Register failed: {(int)registerResponse.StatusCode} {registerResponse.StatusCode}. Body: {registerBody}");

        var auth = JsonSerializer.Deserialize<AuthResponse>(registerBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.Token);

        var meResponse = await client.GetAsync("/api/auth/me");
        var meBody = await meResponse.Content.ReadAsStringAsync();

        // The production bug: a just-issued token rejected by its own [Authorize]
        // handler. Include the body so a 401 (auth) vs 500 (other) is distinguishable.
        Assert.True(meResponse.StatusCode == HttpStatusCode.OK,
            $"Expected 200 OK, got {(int)meResponse.StatusCode} {meResponse.StatusCode}. Body: {meBody}");
    }
}
