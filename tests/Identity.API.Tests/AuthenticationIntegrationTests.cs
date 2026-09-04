using System.Net;
using System.Net.Http.Json;
using GameBackend.Services.Identity.API.Controllers.Dtos;
using GameBackend.Services.Identity.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.API.Tests;

// Exercises the exact same Program.cs wiring (AddJwtBearer + TokenValidationParameters)
// that fails in production with IDX10208. Unlike JwtTokenGeneratorTests, this goes through
// a real HTTP round trip: register -> real generated token -> real [Authorize] handler.
//
// The Postgres DbContext is swapped for SQLite (not the EF InMemory provider):
// PlayerConfiguration maps the inventory as a PrimitiveCollection<List<Guid>>,
// which the non-relational InMemory provider mishandles (that's what turned the
// register call into a 500). SQLite is a real relational provider, so it stores
// that collection as JSON the same way Npgsql stores it as uuid[], which is much
// closer to production behaviour and is Microsoft's recommended provider for
// integration tests for exactly this reason.
public class AuthenticationIntegrationTests : WebApplicationFactory<Program>
{
    // A SQLite in-memory database lives only as long as its connection is open,
    // so the connection has to be held open for the whole factory lifetime rather
    // than let EF open/close it per operation (which would wipe the schema created
    // by EnsureCreated() at startup before the first request ever runs).
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Forces the developer exception page middleware on regardless of ambient
        // ASPNETCORE_ENVIRONMENT in CI, so a 500 comes back with the real exception
        // message/stack in the body instead of an empty Production-mode response.
        builder.UseEnvironment("Development");

        // appsettings.json only ships a placeholder SecretKey ("SET_VIA_USER_
        // SECRETS_OR_ENV", 216 bits) — in production the real key comes from the
        // Jwt__SecretKey env var. That placeholder is too short for HS256 (needs
        // >=256 bits), so tests must supply a valid key of their own. Issuer and
        // Audience are deliberately left to bind from appsettings.json so this
        // test still exercises the real config path.
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "integration-test-signing-key-that-is-long-enough-for-hs256",
            });
        });

        _connection.Open();

        builder.ConfigureServices(services =>
        {
            // AddDbContext<T> doesn't just register DbContextOptions<T> — it also
            // registers the provider configuration (UseNpgsql) as its own
            // IDbContextOptionsConfiguration<T> descriptor. Removing only
            // DbContextOptions<IdentityDbContext> leaves that Npgsql configuration
            // in place, so re-adding with UseSqlite ends up with both providers
            // configured on the same context and EF throws. Strip every descriptor
            // parameterized by IdentityDbContext, not just one type.
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
            _connection.Dispose();
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

        var auth = System.Text.Json.JsonSerializer.Deserialize<AuthResponse>(registerBody,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth!.Token);

        var meResponse = await client.GetAsync("/api/auth/me");
        var meBody = await meResponse.Content.ReadAsStringAsync();

        // This is the exact bug reported in production: a token generated moments
        // ago by this same process is rejected by its own [Authorize] handler.
        // Body is included in the failure message because a non-200 here could be
        // either the auth bug itself (401) or something unrelated blowing up after
        // auth succeeds (500) — those need very different fixes.
        Assert.True(meResponse.StatusCode == HttpStatusCode.OK,
            $"Expected 200 OK, got {(int)meResponse.StatusCode} {meResponse.StatusCode}. Body: {meBody}");
    }
}
