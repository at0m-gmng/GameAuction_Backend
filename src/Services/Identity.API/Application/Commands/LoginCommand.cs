using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Identity.API.Application.Commands;

/// <summary>
/// Команда входа игрока. Возвращает JWT-токен.
/// </summary>
/// <param name="Email">Email для входа.</param>
/// <param name="Password">Пароль в открытом виде.</param>
public sealed record LoginCommand(string Email, string Password) : ICommand<string>;