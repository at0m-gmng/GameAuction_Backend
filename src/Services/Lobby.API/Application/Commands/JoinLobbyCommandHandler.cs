using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.API.Application.Interfaces;

namespace GameBackend.Services.Lobby.API.Application.Commands;

/// <summary>
/// Обработчик команды присоединения к лобби.
/// </summary>
public sealed class JoinLobbyCommandHandler : ICommandHandler<JoinLobbyCommand>
{
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
    /// Загружает лобби, добавляет игрока и сохраняет.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task Handle(JoinLobbyCommand command, CancellationToken cancellationToken)
    {
        var lobby = await _repository.GetByIdAsync(command.LobbyId, cancellationToken)
                    ?? throw new InvalidOperationException($"Лобби {command.LobbyId} не найдено");

        lobby.Join(command.PlayerId);

        await _repository.SaveAsync(lobby, cancellationToken);
    }
}