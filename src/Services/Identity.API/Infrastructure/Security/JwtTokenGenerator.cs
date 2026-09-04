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
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, playerId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}