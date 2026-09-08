using GameBackend.Services.Identity.API.Application.Interfaces;
using GameBackend.Services.Identity.API.Domain.ValueObjects;
using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Identity.API.Application.Commands;

/// <summary>
/// Обработчик списания средств с баланса игрока.
/// </summary>
public sealed class DebitBalanceCommandHandler : ICommandHandler<DebitBalanceCommand>
{
    private readonly IPlayerRepository _repository;

    /// <summary>
    /// Инициализирует обработчик репозиторием.
    /// </summary>
    /// <param name="repository">Репозиторий игроков.</param>
    public DebitBalanceCommandHandler(IPlayerRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Загружает игрока, списывает сумму и сохраняет.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task Handle(DebitBalanceCommand command, CancellationToken cancellationToken)
    {
        var player = await _repository.GetAsync(command.PlayerId, cancellationToken)
                     ?? throw new InvalidOperationException($"Игрок {command.PlayerId} не найден");

        player.SpendFromBalance(new GoldCredits(command.Amount));

        await _repository.SaveAsync(player, cancellationToken);
    }
}
