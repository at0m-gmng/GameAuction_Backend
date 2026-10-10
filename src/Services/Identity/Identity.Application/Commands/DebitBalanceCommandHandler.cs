using GameBackend.Services.Identity.Application.Interfaces;
using GameBackend.Services.Identity.Domain;
using GameBackend.Services.Identity.Domain.ValueObjects;
using GameBackend.SharedKernel.Application;
using GameBackend.SharedKernel.Application.Persistence;

namespace GameBackend.Services.Identity.Application.Commands;

/// <summary>
/// Обработчик списания средств с баланса игрока.
/// </summary>
public sealed class DebitBalanceCommandHandler : ICommandHandler<DebitBalanceCommand>
{
    private readonly IPlayerRepository _repository;
    private readonly IProcessedOperationStore _processedOperations;

    /// <summary>
    /// Инициализирует обработчик репозиторием и журналом идемпотентности.
    /// </summary>
    /// <param name="repository">Репозиторий игроков.</param>
    /// <param name="processedOperations">Журнал идемпотентности.</param>
    public DebitBalanceCommandHandler(IPlayerRepository repository, IProcessedOperationStore processedOperations)
    {
        _repository = repository;
        _processedOperations = processedOperations;
    }

    /// <summary>
    /// Идемпотентно списывает сумму: повтор с тем же ключом пропускается, ключ пишется в том же сохранении.
    /// Ключ необязателен: прямая покупка его не шлёт, дедупликация включается только для расчёта аукциона.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task Handle(DebitBalanceCommand command, CancellationToken cancellationToken)
    {
        bool deduped = !string.IsNullOrEmpty(command.IdempotencyKey);

        if (!deduped || await _processedOperations.FindAsync(command.IdempotencyKey, cancellationToken) is null)
        {
            Player player = await _repository.GetAsync(command.PlayerId, cancellationToken)
                            ?? throw new InvalidOperationException($"Игрок {command.PlayerId} не найден");

            player.SpendFromBalance(new GoldCredits(command.Amount));

            if (deduped)
            {
                _processedOperations.Add(command.IdempotencyKey);
            }

            try
            {
                await _repository.SaveAsync(player, cancellationToken);
            }
            catch (DuplicateEntryException) when (deduped)
            {
            }
        }
    }
}
