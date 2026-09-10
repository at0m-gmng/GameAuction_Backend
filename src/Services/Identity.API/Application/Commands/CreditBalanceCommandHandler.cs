using GameBackend.Services.Identity.API.Application.Interfaces;
using GameBackend.Services.Identity.API.Domain.ValueObjects;
using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Identity.API.Application.Commands;

/// <summary>
/// Обработчик пополнения баланса игрока.
/// </summary>
public sealed class CreditBalanceCommandHandler : ICommandHandler<CreditBalanceCommand>
{
    private readonly IPlayerRepository _repository;

    /// <summary>
    /// Инициализирует обработчик репозиторием.
    /// </summary>
    /// <param name="repository">Репозиторий игроков.</param>
    public CreditBalanceCommandHandler(IPlayerRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Загружает игрока, пополняет баланс и сохраняет.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task Handle(CreditBalanceCommand command, CancellationToken cancellationToken)
    {
        if (command.Amount <= 0)
            throw new InvalidOperationException("Сумма пополнения должна быть больше нуля");

        var player = await _repository.GetAsync(command.PlayerId, cancellationToken)
                     ?? throw new InvalidOperationException($"Игрок {command.PlayerId} не найден");

        player.AddToBalance(new GoldCredits(command.Amount));

        await _repository.SaveAsync(player, cancellationToken);
    }
}
