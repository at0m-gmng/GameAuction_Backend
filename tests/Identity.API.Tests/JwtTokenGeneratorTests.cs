using System.IdentityModel.Tokens.Jwt;
using GameBackend.Services.Identity.API.Infrastructure.Security;
using GameBackend.SharedKernel.Security;

namespace Identity.API.Tests;

public class JwtTokenGeneratorTests
{
    private static JwtTokenGenerator CreateGenerator(JwtSettings settings) => new(settings);

    [Fact]
    public void Generate_ProducesTokenWithIssuerAudienceAndSubject()
    {
        var settings = new JwtSettings
        {
            Issuer = "game-backend",
            Audience = "game-backend-clients",
            SecretKey = "this-is-a-test-secret-key-at-least-32-bytes-long",
            ExpiryMinutes = 60,
        };
        var playerId = Guid.NewGuid();

        var token = CreateGenerator(settings).Generate(playerId);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // This is the exact regression that shipped once already: a token
        // generated with no exception thrown, but missing iss/aud, which
        // then made every single call to an [Authorize] endpoint return 401
        // regardless of how fresh the token was.
        Assert.Equal("game-backend", jwt.Issuer);
        Assert.Contains("game-backend-clients", jwt.Audiences);
        Assert.Equal(playerId.ToString(), jwt.Subject);
    }

    [Fact]
    public void Generate_FallsBackToDefaultIssuerAudience_WhenSettingsAreEmpty()
    {
        var settings = new JwtSettings
        {
            Issuer = "",
            Audience = "",
            SecretKey = "this-is-a-test-secret-key-at-least-32-bytes-long",
            ExpiryMinutes = 60,
        };

        var token = CreateGenerator(settings).Generate(Guid.NewGuid());
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("game-backend", jwt.Issuer);
        Assert.Contains("game-backend-clients", jwt.Audiences);
    }

    [Fact]
    public void Generate_Throws_WhenSecretKeyMissing()
    {
        var settings = new JwtSettings
        {
            Issuer = "game-backend",
            Audience = "game-backend-clients",
            SecretKey = "",
            ExpiryMinutes = 60,
        };

        Assert.Throws<InvalidOperationException>(() => CreateGenerator(settings).Generate(Guid.NewGuid()));
    }
}
