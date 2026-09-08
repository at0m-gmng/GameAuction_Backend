using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.API.Application.Interfaces;
using GameBackend.Services.Lobby.API.Infrastructure.ExternalServices;
using Microsoft.Extensions.Logging;

namespace GameBackend.Services.Lobby.API.Application.Commands;

/// <summary>
/// Обработчик команды завершения аукциона.
/// </summary>
public sealed class CompleteAuctionCommandHandler : ICommandHandler<CompleteAuctionCommand>
{
    private readonly ILobbyRepository _repository;
    private readonly IIdentityServiceClient _identityClient;
    private readonly ICatalogServiceClient _catalogClient;
    private readonly ILogger<CompleteAuctionCommandHandler> _logger;

    /// <summary>
    /// Инициализирует обработчик зависимостями.
    /// </summary>
    /// <param name="repository">Репозиторий лобби.</param>
    /// <param name="identityClient">Клиент к Identity.API (списание баланса).</param>
    /// <param name="catalogClient">Клиент к Catalog.API (передача предмета).</param>
    /// <param name="logger">Логгер.</param>
    public CompleteAuctionCommandHandler(
        ILobbyRepository repository,
        IIdentityServiceClient identityClient,
        ICatalogServiceClient catalogClient,
        ILogger<CompleteAuctionCommandHandler> logger)
    {
        _repository = repository;
        _identityClient = identityClient;
        _catalogClient = catalogClient;
        _logger = logger;
    }

    /// <summary>
    /// Загружает лобби, завершает аукцион, сохраняет и рассчитывается с победителем.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task Handle(CompleteAuctionCommand command, CancellationToken cancellationToken)
    {
        var lobby = await _repository.GetByIdAsync(command.LobbyId, cancellationToken)
                    ?? throw new InvalidOperationException($"Лобби {command.LobbyId} не найдено");

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
