using GameBackend.Services.Lobby.API.Application.Interfaces;
using GameBackend.Services.Lobby.API.Domain;
using GameBackend.Services.Lobby.API.Infrastructure.ExternalServices;
using Microsoft.Extensions.Logging;

namespace GameBackend.Services.Lobby.API.Application.Services;

/// <summary>
/// Завершает аукцион и рассчитывается с победителем — доменом или лениво при чтении просроченного лобби.
/// </summary>
public sealed class AuctionCompletionService
{
    private readonly ILobbyRepository _repository;
    private readonly IIdentityServiceClient _identityClient;
    private readonly ICatalogServiceClient _catalogClient;
    private readonly ILogger<AuctionCompletionService> _logger;

    /// <summary>
    /// Инициализирует сервис зависимостями.
    /// </summary>
    /// <param name="repository">Репозиторий лобби.</param>
    /// <param name="identityClient">Клиент к Identity.API (списание баланса).</param>
    /// <param name="catalogClient">Клиент к Catalog.API (передача предмета).</param>
    /// <param name="logger">Логгер.</param>
    public AuctionCompletionService(
        ILobbyRepository repository,
        IIdentityServiceClient identityClient,
        ICatalogServiceClient catalogClient,
        ILogger<AuctionCompletionService> logger)
    {
        _repository = repository;
        _identityClient = identityClient;
        _catalogClient = catalogClient;
        _logger = logger;
    }

    /// <summary>
    /// Завершает лобби, если оно в статусе Bidding и время аукциона истекло. Иначе ничего не делает.
    /// </summary>
    /// <param name="lobby">Уже загруженный агрегат лобби.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task CompleteIfExpiredAsync(LobbyAggregate lobby, CancellationToken cancellationToken)
    {
        if (lobby.Status != LobbyStatus.Bidding || lobby.EndsAt is null || lobby.EndsAt > DateTime.UtcNow)
            return;

        await CompleteAsync(lobby, cancellationToken);
    }

    /// <summary>
    /// Завершает аукцион, сохраняет лобби и best-effort рассчитывается с победителем.
    /// </summary>
    /// <param name="lobby">Уже загруженный агрегат лобби.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task CompleteAsync(LobbyAggregate lobby, CancellationToken cancellationToken)
    {
        lobby.Complete();

        await _repository.SaveAsync(lobby, cancellationToken);

        if (lobby.WinnerId is null || lobby.CurrentBid is null)
            return;

        // NOTE: best-effort — сбой оплаты/выдачи не должен откатывать сам факт завершения аукциона.
        try
        {
            await _identityClient.DebitAsync(lobby.WinnerId.Value, lobby.CurrentBid.Amount, cancellationToken);
            await _catalogClient.AwardItemAsync(lobby.ItemId, lobby.WinnerId.Value, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось провести расчёт по лобби {LobbyId} с победителем {WinnerId}", lobby.Id, lobby.WinnerId);
        }
    }
}
