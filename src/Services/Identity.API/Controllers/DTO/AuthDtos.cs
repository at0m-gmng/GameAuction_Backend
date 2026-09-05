namespace GameBackend.Services.Identity.API.Controllers.Dtos;

/// <summary>
/// Тело запроса на регистрацию.
/// </summary>
/// <param name="Nickname">Никнейм игрока.</param>
/// <param name="Email">Email для входа.</param>
/// <param name="Password">Пароль в открытом виде.</param>
public sealed record RegisterRequest(string Nickname, string Email, string Password);

/// <summary>
/// Тело запроса на вход.
/// </summary>
/// <param name="Email">Email для входа.</param>
/// <param name="Password">Пароль в открытом виде.</param>
public sealed record LoginRequest(string Email, string Password);

/// <summary>
/// Ответ с JWT-токеном.
/// </summary>
/// <param name="Token">JWT-токен.</param>
public sealed record AuthResponse(string Token);