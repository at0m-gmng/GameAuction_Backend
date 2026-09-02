using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.API.Application.Interfaces;

namespace GameBackend.Services.Lobby.API.Application.Commands;

/// <summary>
/// Обработчик команды запуска аукциона.
/// </summary>
public sealed class StartAuctionCommandHandler : ICommandHandler<StartAuctionCommand>
{
    private readonly ILobbyRepository _repository;

    /// <summary>
    /// Инициализирует обработчик репозиторием.
    /// </summary>
    /// <param name="repository">Репозиторий лобби.</param>
    public StartAuctionCommandHandler(ILobbyRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Загружает лобби, запускает аукцион и сохраняет.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task Handle(StartAuctionCommand command, CancellationToken cancellationToken)
    {
        var lobby = await _repository.GetByIdAsync(command.LobbyId, cancellationToken)
                    ?? throw new InvalidOperationException($"Лобби {command.LobbyId} не найдено");

        lobby.StartAuction(command.Duration);

        await _repository.SaveAsync(lobby, cancellationToken);
    }
}