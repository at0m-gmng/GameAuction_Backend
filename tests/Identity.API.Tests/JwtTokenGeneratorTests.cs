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

        Assert.Equal("game-backend", jwt.Issuer);
        Assert.Contains("game-backend-clients", jwt.Audiences);
        Assert.Equal(playerId.ToString(), jwt.Subject);
        Assert.Single(jwt.Audiences);
    }
}
