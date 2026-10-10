using GameBackend.Services.Lobby.Application.Interfaces;
using GameBackend.Services.Lobby.Application.Settlement;
using GameBackend.Services.Lobby.Domain;
using GameBackend.SharedKernel.Application.Persistence;

namespace GameBackend.Services.Lobby.Application.Services;

/// <summary>
/// Завершает аукцион — доменом или лениво при чтении просроченного лобби — и ставит расчёт в outbox.
/// </summary>
public sealed class AuctionCompletionService
{
    private readonly ILobbyRepository _repository;
    private readonly ISettlementOutbox _outbox;

    /// <summary>
    /// Инициализирует сервис зависимостями.
    /// </summary>
    /// <param name="repository">Репозиторий лобби.</param>
    /// <param name="outbox">Outbox расчётов.</param>
    public AuctionCompletionService(ILobbyRepository repository, ISettlementOutbox outbox)
    {
        _repository = repository;
        _outbox = outbox;
    }

    /// <summary>
    /// Обрабатывает истёкшее Bidding-лобби: без ставок — сброс в Gathering, со ставками — завершение и расчёт.
    /// </summary>
    /// <param name="lobby">Уже загруженный агрегат лобби.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task CompleteIfExpiredAsync(LobbyAggregate lobby, CancellationToken cancellationToken)
    {
        if (lobby.Status == LobbyStatus.Bidding && lobby.EndsAt is not null && lobby.EndsAt <= DateTime.UtcNow)
        {
            if (lobby.CurrentBid is null)
            {
                lobby.ExpireWithoutBids();
                await _repository.SaveAsync(lobby, cancellationToken);
            }
            else
            {
                await CompleteAsync(lobby, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Завершает аукцион и сохраняет лобби вместе с расчётом в outbox — атомарно, одним сохранением.
    /// Сам расчёт (предмет, списание, начисление) проводит фоновый обработчик outbox с повторами.
    /// Если параллельный запрос уже завершил это лобби, уникальный индекс outbox отклонит дубль — это не ошибка.
    /// </summary>
    /// <param name="lobby">Уже загруженный агрегат лобби.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task CompleteAsync(LobbyAggregate lobby, CancellationToken cancellationToken)
    {
        lobby.Complete();

        if (lobby.WinnerId.HasValue && lobby.CurrentBid is not null)
        {
            _outbox.Enqueue(new AuctionSettlement(lobby.Id, lobby.ItemId, lobby.WinnerId.Value, lobby.CurrentBid.Amount));
        }

        try
        {
            await _repository.SaveAsync(lobby, cancellationToken);
        }
        catch (DuplicateEntryException)
        {
        }
    }
}
