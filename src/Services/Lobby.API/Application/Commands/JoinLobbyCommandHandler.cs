using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.API.Application.Interfaces;

namespace GameBackend.Services.Lobby.API.Application.Commands;

/// <summary>
/// Обработчик команды присоединения к лобби.
/// </summary>
public sealed class JoinLobbyCommandHandler : ICommandHandler<JoinLobbyCommand>
{
    // NOTE: сервис не имеет отдельного эндпоинта/UI для настройки длительности за лобби — фиксированное окно ставок.
    private static readonly TimeSpan AuctionDuration = TimeSpan.FromMinutes(5);

    private readonly ILobbyRepository _repository;

    /// <summary>
    /// Инициализирует обработчик репозиторием.
    /// </summary>
    /// <param name="repository">Репозиторий лобби.</param>
    public JoinLobbyCommandHandler(ILobbyRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Загружает лобби, добавляет игрока, автоматически стартует аукцион при заполнении и сохраняет.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task Handle(JoinLobbyCommand command, CancellationToken cancellationToken)
    {
        var lobby = await _repository.GetByIdAsync(command.LobbyId, cancellationToken)
                    ?? throw new InvalidOperationException($"Лобби {command.LobbyId} не найдено");

        lobby.Join(command.PlayerId);

        if (lobby.Participants.Count >= lobby.MaxParticipants)
            lobby.StartAuction(AuctionDuration);

        await _repository.SaveAsync(lobby, cancellationToken);
    }
}