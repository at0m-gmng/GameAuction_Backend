using GameBackend.Services.Identity.Application.Interfaces;
using GameBackend.Services.Identity.Domain;
using GameBackend.Services.Identity.Domain.ValueObjects;
using GameBackend.SharedKernel.Application;
using GameBackend.SharedKernel.Application.Persistence;

namespace GameBackend.Services.Identity.Application.Commands;

/// <summary>
/// Обработчик пополнения баланса игрока.
/// </summary>
public sealed class CreditBalanceCommandHandler : ICommandHandler<CreditBalanceCommand>
{
    private readonly IPlayerRepository _repository;
    private readonly IProcessedOperationStore _processedOperations;

    /// <summary>
    /// Инициализирует обработчик репозиторием и журналом идемпотентности.
    /// </summary>
    /// <param name="repository">Репозиторий игроков.</param>
    /// <param name="processedOperations">Журнал идемпотентности.</param>
    public CreditBalanceCommandHandler(IPlayerRepository repository, IProcessedOperationStore processedOperations)
    {
        _repository = repository;
        _processedOperations = processedOperations;
    }

    /// <summary>
    /// Идемпотентно начисляет сумму: повтор с тем же ключом пропускается, ключ пишется в том же сохранении.
    /// Ключ необязателен: дедупликация включается только для расчёта аукциона.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task Handle(CreditBalanceCommand command, CancellationToken cancellationToken)
    {
        if (command.Amount <= 0)
        {
            throw new InvalidOperationException("Сумма пополнения должна быть больше нуля");
        }

        bool deduped = !string.IsNullOrEmpty(command.IdempotencyKey);

        if (!deduped || await _processedOperations.FindAsync(command.IdempotencyKey, cancellationToken) is null)
        {
            Player player = await _repository.GetAsync(command.PlayerId, cancellationToken)
                            ?? throw new InvalidOperationException($"Игрок {command.PlayerId} не найден");

            player.AddToBalance(new GoldCredits(command.Amount));

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
