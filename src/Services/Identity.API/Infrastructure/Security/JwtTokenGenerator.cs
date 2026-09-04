using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using GameBackend.SharedKernel.Security;

namespace GameBackend.Services.Identity.API.Infrastructure.Security;

/// <summary>
/// Генератор JWT-токенов для игроков.
/// Кладёт PlayerId в стандартный claim "sub" (subject).
/// </summary>
public sealed class JwtTokenGenerator
{
    private readonly JwtSettings _settings;

    /// <summary>
    /// Инициализирует генератор настройками.
    /// </summary>
    /// <param name="settings">Настройки JWT из конфигурации.</param>
    public JwtTokenGenerator(JwtSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Создаёт подписанный JWT для указанного игрока.
    /// </summary>
    /// <param name="playerId">Идентификатор игрока.</param>
    /// <returns>JWT-токен в виде строки.</returns>
    public string Generate(Guid playerId)
    {
        if (string.IsNullOrEmpty(_settings.SecretKey))
            throw new InvalidOperationException("Jwt:SecretKey is not configured — check Jwt__SecretKey on the deployed environment.");

        // Defensive fallback: if Issuer/Audience somehow arrive empty from
        // configuration, fall back to the values committed in appsettings.json
        // rather than emitting a token with no iss/aud (which validation
        // would reject) or throwing (Claim's constructor rejects null values).
        var issuer = string.IsNullOrEmpty(_settings.Issuer) ? "game-backend" : _settings.Issuer;
        var audience = string.IsNullOrEmpty(_settings.Audience) ? "game-backend-clients" : _settings.Audience;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Issuer/audience are added both as constructor params and as explicit
        // claims below — belt and suspenders, since a prior build silently
        // produced tokens missing "iss"/"aud", which made every validation
        // request come back 401 regardless of how fresh the token was.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, playerId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iss, issuer),
            new Claim(JwtRegisteredClaimNames.Aud, audience),
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}