using GameBackend.Services.Lobby.Application.Interfaces;
using GameBackend.Services.Lobby.Domain;
using GameBackend.SharedKernel.Application;
using GameBackend.SharedKernel.Application.Persistence;

namespace GameBackend.Services.Lobby.Application.Commands;

/// <summary>
/// Обработчик команды создания лобби.
/// </summary>
public sealed class CreateLobbyCommandHandler : ICommandHandler<CreateLobbyCommand, Guid>
{
    private readonly ILobbyRepository _repository;

    /// <summary>
    /// Инициализирует обработчик репозиторием.
    /// </summary>
    /// <param name="repository">Репозиторий лобби.</param>
    public CreateLobbyCommandHandler(ILobbyRepository repository) => _repository = repository;

    /// <summary>
    /// Создаёт лобби через фабричный метод и сохраняет его.
    /// При одновременных запросах на создание лобби для одного предмета уникальный индекс БД
    /// предотвратит дубликаты — проигравший запрос повторит lookup и вернёт уже созданное лобби.
    /// </summary>
    /// <param name="command">Команда создания лобби.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Идентификатор созданного лобби.</returns>
    public async Task<Guid> Handle(CreateLobbyCommand command, CancellationToken cancellationToken)
    {
        LobbyAggregate? existing = await _repository.GetOpenLobbyByItemIdAsync(command.ItemId, cancellationToken);
        Guid lobbyId;

        if (existing is not null)
        {
            lobbyId = existing.Id;
        }
        else
        {
            LobbyAggregate lobby = LobbyAggregate.Create(
                command.ItemId,
                command.ItemName,
                command.ItemImageUrl,
                command.ItemRarity,
                command.StartingPrice,
                command.MaxParticipants);

            try
            {
                await _repository.SaveAsync(lobby, cancellationToken);
                lobbyId = lobby.Id;
            }
            catch (DuplicateEntryException)
            {
                LobbyAggregate winner = await _repository.GetOpenLobbyByItemIdAsync(command.ItemId, cancellationToken)
                                        ?? throw new InvalidOperationException($"Лобби для предмета {command.ItemId} не найдено после конфликта создания");
                lobbyId = winner.Id;
            }
        }

        return lobbyId;
    }
}
