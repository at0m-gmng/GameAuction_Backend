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

public class AuthenticationIntegrationTests : WebApplicationFactory<Program>
{
    // NOTE: valid key must come via env var — Program.cs reads config before
    // app.Build(), too early for the factory's ConfigureAppConfiguration.
    private const string TestSecretKey = "integration-test-signing-key-that-is-long-enough-for-hs256";

    // NOTE: a SQLite in-memory DB lives only while its connection is open.
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public AuthenticationIntegrationTests()
    {
        Environment.SetEnvironmentVariable("Jwt__SecretKey", TestSecretKey);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        _connection.Open();

        builder.ConfigureServices(services =>
        {
            // NOTE: AddDbContext registers several descriptors keyed by
            // IdentityDbContext; strip them all before re-adding SQLite.
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

        Assert.True(meResponse.StatusCode == HttpStatusCode.OK,
            $"Expected 200 OK, got {(int)meResponse.StatusCode} {meResponse.StatusCode}. Body: {meBody}");
    }
}
