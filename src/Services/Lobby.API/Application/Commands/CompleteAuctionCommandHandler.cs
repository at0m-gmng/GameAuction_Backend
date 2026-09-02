using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.API.Application.Interfaces;

namespace GameBackend.Services.Lobby.API.Application.Commands;

/// <summary>
/// Обработчик команды завершения аукциона.
/// </summary>
public sealed class CompleteAuctionCommandHandler : ICommandHandler<CompleteAuctionCommand>
{
    private readonly ILobbyRepository _repository;

    /// <summary>
    /// Инициализирует обработчик репозиторием.
    /// </summary>
    /// <param name="repository">Репозиторий лобби.</param>
    public CompleteAuctionCommandHandler(ILobbyRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Загружает лобби, завершает аукцион и сохраняет.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task Handle(CompleteAuctionCommand command, CancellationToken cancellationToken)
    {
        var lobby = await _repository.GetByIdAsync(command.LobbyId, cancellationToken)
                    ?? throw new InvalidOperationException($"Лобби {command.LobbyId} не найдено");

        lobby.Complete();

        await _repository.SaveAsync(lobby, cancellationToken);
    }
}