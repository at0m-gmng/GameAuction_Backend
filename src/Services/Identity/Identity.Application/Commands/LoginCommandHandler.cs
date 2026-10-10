using GameBackend.Services.Identity.Application.Interfaces;
using GameBackend.Services.Identity.Application.Services;
using GameBackend.Services.Identity.Domain.ValueObjects;
using GameBackend.Services.Identity.Application.Configuration;
using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Identity.Application.Commands;

/// <summary>
/// Обработчик логина: проверяет пароль и возвращает JWT.
/// </summary>
public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, string>
{
    // NOTE: фиктивный хеш — заставляет Verify всегда делать PBKDF2-проход (анти-timing-атака на email).
    private const string DummyPasswordHash =
        "AAAAAAAAAAAAAAAAAAAAAA==.AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";

    private readonly IPlayerRepository _repository;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenGenerator _jwt;
    private readonly WelcomeGiftFulfiller _welcomeGift;
    private readonly EconomySettings _economy;

    /// <summary>
    /// Инициализирует обработчик зависимостями.
    /// </summary>
    /// <param name="repository">Репозиторий игроков.</param>
    /// <param name="hasher">Хешер паролей.</param>
    /// <param name="jwt">Генератор JWT.</param>
    /// <param name="welcomeGift">Довыдача приветственного подарка, если он не был выдан при регистрации.</param>
    /// <param name="economy">Игровые экономические параметры.</param>
    public LoginCommandHandler(
        IPlayerRepository repository,
        IPasswordHasher hasher,
        ITokenGenerator jwt,
        WelcomeGiftFulfiller welcomeGift,
        EconomySettings economy)
    {
        _repository = repository;
        _hasher = hasher;
        _jwt = jwt;
        _welcomeGift = welcomeGift;
        _economy = economy;
    }

    /// <summary>
    /// Проверяет учётные данные, возвращает JWT; нормализует email, чтобы регистр не влиял на поиск игрока.
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

        // NOTE: довыдача для игроков, зарегистрированных до появления стартового баланса.
        if (player.GrantStartingBalanceIfNeeded(new GoldCredits(_economy.StartingBalance)))
            await _repository.SaveAsync(player, cancellationToken);

        return _jwt.Generate(player.Id);
    }
}