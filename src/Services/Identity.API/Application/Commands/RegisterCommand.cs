using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Identity.API.Application.Commands;

/// <summary>
/// Команда регистрации нового игрока. Возвращает JWT-токен.
/// </summary>
/// <param name="Nickname">Никнейм игрока.</param>
/// <param name="Email">Email для входа.</param>
/// <param name="Password">Пароль в открытом виде (будет захеширован).</param>
public sealed record RegisterCommand(string Nickname, string Email, string Password) : ICommand<string>;