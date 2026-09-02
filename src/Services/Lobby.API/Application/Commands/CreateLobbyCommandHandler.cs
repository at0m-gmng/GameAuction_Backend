using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.API.Application.Interfaces;
using GameBackend.Services.Lobby.API.Domain;

namespace GameBackend.Services.Lobby.API.Application.Commands;

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
    public CreateLobbyCommandHandler(ILobbyRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Создаёт лобби через фабричный метод и сохраняет его.
    /// </summary>
    /// <param name="command">Команда создания лобби.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Идентификатор созданного лобби.</returns>
    public async Task<Guid> Handle(CreateLobbyCommand command, CancellationToken cancellationToken)
    {
        var lobby = Lobby.Create(
            command.ItemId,
            command.ItemName,
            command.ItemImageUrl,
            command.StartingPrice,
            command.MaxParticipants);

        await _repository.SaveAsync(lobby, cancellationToken);

        return lobby.Id;
    }
}