using GameBackend.Services.Identity.API.Application.Interfaces;
using GameBackend.Services.Identity.API.Application.Services;
using GameBackend.Services.Identity.API.Domain;
using GameBackend.Services.Identity.API.Domain.ValueObjects;
using GameBackend.Services.Identity.API.Infrastructure.Configuration;
using GameBackend.Services.Identity.API.Infrastructure.Security;
using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Identity.API.Application.Commands;

/// <summary>
/// Обработчик регистрации: создаёт игрока, хеширует пароль, возвращает JWT.
/// </summary>
public sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, string>
{
    private readonly IPlayerRepository _repository;
    private readonly PasswordHasher _hasher;
    private readonly JwtTokenGenerator _jwt;
    private readonly WelcomeGiftFulfiller _welcomeGift;
    private readonly EconomySettings _economy;

    /// <summary>
    /// Инициализирует обработчик зависимостями.
    /// </summary>
    /// <param name="repository">Репозиторий игроков.</param>
    /// <param name="hasher">Хешер паролей.</param>
    /// <param name="jwt">Генератор JWT.</param>
    /// <param name="welcomeGift">Выдача приветственного подарка.</param>
    /// <param name="economy">Игровые экономические параметры.</param>
    public RegisterCommandHandler(
        IPlayerRepository repository,
        PasswordHasher hasher,
        JwtTokenGenerator jwt,
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
    /// Регистрирует игрока и возвращает JWT-токен.
    /// </summary>
    /// <param name="command">Команда регистрации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>JWT-токен.</returns>
    public async Task<string> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Email.ToLowerInvariant();

        var existing = await _repository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException("Игрок с таким email уже зарегистрирован");

        var passwordHash = _hasher.Hash(command.Password);

        var player = Player.Register(command.Nickname, command.Email, passwordHash, new GoldCredits(_economy.StartingBalance));

        await _repository.SaveAsync(player, cancellationToken);

        await _welcomeGift.EnsureGrantedAsync(player, cancellationToken);

        return _jwt.Generate(player.Id);
    }
}