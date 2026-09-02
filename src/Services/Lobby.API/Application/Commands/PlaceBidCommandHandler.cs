using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.API.Application.Interfaces;

namespace GameBackend.Services.Lobby.API.Application.Commands;

/// <summary>
/// Обработчик команды ставки.
/// </summary>
public sealed class PlaceBidCommandHandler : ICommandHandler<PlaceBidCommand>
{
    private readonly ILobbyRepository _repository;

    /// <summary>
    /// Инициализирует обработчик репозиторием.
    /// </summary>
    /// <param name="repository">Репозиторий лобби.</param>
    public PlaceBidCommandHandler(ILobbyRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Загружает лобби, регистрирует ставку и сохраняет.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task Handle(PlaceBidCommand command, CancellationToken cancellationToken)
    {
        var lobby = await _repository.GetByIdAsync(command.LobbyId, cancellationToken)
                    ?? throw new InvalidOperationException($"Лобби {command.LobbyId} не найдено");

        lobby.PlaceBid(command.PlayerId, command.Amount);

        await _repository.SaveAsync(lobby, cancellationToken);
    }
}