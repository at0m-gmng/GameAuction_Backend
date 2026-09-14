using GameBackend.Services.Identity.API.Application.Interfaces;
using GameBackend.Services.Identity.API.Domain.ValueObjects;
using GameBackend.Services.Identity.API.Infrastructure.Persistence;
using GameBackend.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Services.Identity.API.Application.Commands;

/// <summary>
/// Обработчик пополнения баланса игрока.
/// </summary>
public sealed class CreditBalanceCommandHandler : ICommandHandler<CreditBalanceCommand>
{
    private readonly IPlayerRepository _repository;
    private readonly IdentityDbContext _context;

    /// <summary>
    /// Инициализирует обработчик репозиторием и контекстом БД (для журнала идемпотентности).
    /// </summary>
    /// <param name="repository">Репозиторий игроков.</param>
    /// <param name="context">Контекст БД идентификации.</param>
    public CreditBalanceCommandHandler(IPlayerRepository repository, IdentityDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    /// <summary>
    /// Идемпотентно пополняет баланс: повтор с тем же ключом пропускается, ключ пишется в той же транзакции.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task Handle(CreditBalanceCommand command, CancellationToken cancellationToken)
    {
        if (command.Amount <= 0)
            throw new InvalidOperationException("Сумма пополнения должна быть больше нуля");

        // NOTE: ключ необязателен — дедуп включается только когда вызывающий его прислал (расчёт аукциона).
        var deduped = !string.IsNullOrEmpty(command.IdempotencyKey);
        if (deduped && await _context.ProcessedOperations.AnyAsync(x => x.Key == command.IdempotencyKey, cancellationToken))
            return;

        var player = await _repository.GetAsync(command.PlayerId, cancellationToken)
                     ?? throw new InvalidOperationException($"Игрок {command.PlayerId} не найден");

        player.AddToBalance(new GoldCredits(command.Amount));

        if (deduped)
            _context.ProcessedOperations.Add(new ProcessedOperation(command.IdempotencyKey));

        try
        {
            await _repository.SaveAsync(player, cancellationToken);
        }
        catch (DbUpdateException) when (deduped)
        {
            // NOTE: гонка — ключ уже вставлен другим запросом; SaveChanges откатывает и начисление.
        }
    }
}
