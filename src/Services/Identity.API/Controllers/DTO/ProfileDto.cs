namespace GameBackend.Services.Identity.API.Controllers.Dtos;

/// <summary>
/// Профиль игрока для ответа на /api/auth/me.
/// </summary>
/// <param name="PlayerId">Идентификатор игрока.</param>
/// <param name="Nickname">Никнейм.</param>
/// <param name="Email">Email.</param>
/// <param name="Balance">Баланс в золотых кредитах.</param>
/// <param name="CreatedAt">Дата регистрации (UTC).</param>
public sealed record ProfileDto(Guid PlayerId, string Nickname, string Email, decimal Balance, DateTime CreatedAt);