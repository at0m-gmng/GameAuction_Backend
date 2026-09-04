using System.Net;
using System.Net.Http.Json;
using GameBackend.Services.Identity.API.Controllers.Dtos;
using GameBackend.Services.Identity.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.API.Tests;

// Exercises the exact same Program.cs wiring (AddJwtBearer + TokenValidationParameters)
// that fails in production with IDX10208. Unlike JwtTokenGeneratorTests, this goes through
// a real HTTP round trip: register -> real generated token -> real [Authorize] handler.
// Only the Postgres DbContext is swapped for InMemory, since EnsureCreated() at startup
// needs a database to talk to and CI has no Postgres available.
public class AuthenticationIntegrationTests : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<IdentityDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<IdentityDbContext>(options =>
                options.UseInMemoryDatabase($"IdentityTestDb-{Guid.NewGuid()}"));
        });
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
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth!.Token);

        var meResponse = await client.GetAsync("/api/auth/me");

        // This is the exact bug reported in production: a token generated moments
        // ago by this same process is rejected by its own [Authorize] handler.
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
    }
}
