using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.API.Application.Interfaces;
using GameBackend.Services.Lobby.API.Application.Services;
using GameBackend.Services.Lobby.API.Domain;

namespace GameBackend.Services.Lobby.API.Application.Commands;

/// <summary>
/// Обработчик команды присоединения к лобби.
/// </summary>
public sealed class JoinLobbyCommandHandler : ICommandHandler<JoinLobbyCommand>
{
    private static readonly TimeSpan AuctionDuration = TimeSpan.FromSeconds(60);

    private readonly ILobbyRepository _repository;
    private readonly AuctionCompletionService _auctionCompletion;

    /// <summary>
    /// Инициализирует обработчик зависимостями.
    /// </summary>
    /// <param name="repository">Репозиторий лобби.</param>
    /// <param name="auctionCompletion">Сервис завершения просроченных аукционов.</param>
    public JoinLobbyCommandHandler(ILobbyRepository repository, AuctionCompletionService auctionCompletion)
    {
        _repository = repository;
        _auctionCompletion = auctionCompletion;
    }

    /// <summary>
    /// Добавляет игрока: первый вход стартует торги, вход во время торгов продлевает таймер.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task Handle(JoinLobbyCommand command, CancellationToken cancellationToken)
    {
        var lobby = await _repository.GetByIdAsync(command.LobbyId, cancellationToken)
                    ?? throw new InvalidOperationException($"Лобби {command.LobbyId} не найдено");

        // NOTE: сначала разбираемся с просрочкой — иначе join мог бы "оживить" уже протухший раунд.
        await _auctionCompletion.CompleteIfExpiredAsync(lobby, cancellationToken);

        var wasGathering = lobby.Status == LobbyStatus.Gathering;

        // NOTE: время двигаем только за реально нового игрока раунда — иначе повторные входы бесконечно продлевают таймер.
        var isNewArrival = lobby.Join(command.PlayerId);

        if (isNewArrival)
        {
            if (wasGathering)
                lobby.StartAuction(AuctionDuration);
            else
                lobby.ExtendOnJoin(AuctionDuration);
        }

        await _repository.SaveAsync(lobby, cancellationToken);
    }
}