using GameBackend.Services.Identity.API.Application.Interfaces;
using GameBackend.Services.Identity.API.Domain.ValueObjects;
using GameBackend.Services.Identity.API.Infrastructure.Persistence;
using GameBackend.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Services.Identity.API.Application.Commands;

/// <summary>
/// Обработчик списания средств с баланса игрока.
/// </summary>
public sealed class DebitBalanceCommandHandler : ICommandHandler<DebitBalanceCommand>
{
    private readonly IPlayerRepository _repository;
    private readonly IdentityDbContext _context;

    /// <summary>
    /// Инициализирует обработчик репозиторием и контекстом БД (для журнала идемпотентности).
    /// </summary>
    /// <param name="repository">Репозиторий игроков.</param>
    /// <param name="context">Контекст БД идентификации.</param>
    public DebitBalanceCommandHandler(IPlayerRepository repository, IdentityDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    /// <summary>
    /// Идемпотентно списывает сумму: повтор с тем же ключом пропускается, ключ пишется в той же транзакции.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task Handle(DebitBalanceCommand command, CancellationToken cancellationToken)
    {
        // NOTE: ключ необязателен — прямая покупка его не шлёт; дедуп включается только для расчёта аукциона.
        var deduped = !string.IsNullOrEmpty(command.IdempotencyKey);
        if (deduped && await _context.ProcessedOperations.AnyAsync(x => x.Key == command.IdempotencyKey, cancellationToken))
            return;

        var player = await _repository.GetAsync(command.PlayerId, cancellationToken)
                     ?? throw new InvalidOperationException($"Игрок {command.PlayerId} не найден");

        player.SpendFromBalance(new GoldCredits(command.Amount));

        if (deduped)
            _context.ProcessedOperations.Add(new ProcessedOperation(command.IdempotencyKey));

        try
        {
            await _repository.SaveAsync(player, cancellationToken);
        }
        catch (DbUpdateException) when (deduped)
        {
            // NOTE: гонка — ключ уже вставлен другим запросом; SaveChanges откатывает и списание.
        }
    }
}
