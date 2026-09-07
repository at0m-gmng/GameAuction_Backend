using GameBackend.Services.Identity.API.Application.Interfaces;
using GameBackend.Services.Identity.API.Application.Services;
using GameBackend.Services.Identity.API.Infrastructure.Security;
using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Identity.API.Application.Commands;

/// <summary>
/// Обработчик логина: проверяет пароль и возвращает JWT.
/// </summary>
public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, string>
{
    // NOTE: фиктивный хеш — заставляет Verify всегда делать PBKDF2-проход (анти-timing-атака на email).
    private const string DummyPasswordHash =
        "AAAAAAAAAAAAAAAAAAAAAA==.AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";

    private readonly IPlayerRepository _repository;
    private readonly PasswordHasher _hasher;
    private readonly JwtTokenGenerator _jwt;
    private readonly WelcomeGiftFulfiller _welcomeGift;

    /// <summary>
    /// Инициализирует обработчик зависимостями.
    /// </summary>
    /// <param name="repository">Репозиторий игроков.</param>
    /// <param name="hasher">Хешер паролей.</param>
    /// <param name="jwt">Генератор JWT.</param>
    /// <param name="welcomeGift">Довыдача приветственного подарка, если он не был выдан при регистрации.</param>
    public LoginCommandHandler(
        IPlayerRepository repository,
        PasswordHasher hasher,
        JwtTokenGenerator jwt,
        WelcomeGiftFulfiller welcomeGift)
    {
        _repository = repository;
        _hasher = hasher;
        _jwt = jwt;
        _welcomeGift = welcomeGift;
    }

    /// <summary>
    /// Проверяет учётные данные и возвращает JWT-токен.
    /// Нормализует email для поиска, чтобы aBc@gmail.com и ABC@gmail.com находили одного игрока.
    /// </summary>
    /// <param name="command">Команда логина.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>JWT-токен.</returns>
    public async Task<string> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Email.ToLowerInvariant();
        var player = await _repository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);

        var passwordValid = _hasher.Verify(command.Password, player?.PasswordHash ?? DummyPasswordHash);

        if (player is null || !passwordValid)
            throw new InvalidOperationException("Неверный email или пароль");

        await _welcomeGift.EnsureGrantedAsync(player, cancellationToken);

        return _jwt.Generate(player.Id);
    }
}