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
        // Forces the developer exception page middleware on regardless of ambient
        // ASPNETCORE_ENVIRONMENT in CI, so a 500 comes back with the real exception
        // message/stack in the body instead of an empty Production-mode response.
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // AddDbContext<T> doesn't just register DbContextOptions<T> — it also
            // registers the provider configuration (UseNpgsql) as its own
            // IDbContextOptionsConfiguration<T> descriptor. Removing only
            // DbContextOptions<IdentityDbContext> leaves that Npgsql configuration
            // in place, so re-adding with UseInMemoryDatabase ends up with both
            // providers configured on the same context and EF throws. Strip every
            // descriptor parameterized by IdentityDbContext, not just one type.
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
