using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.API.Application.Interfaces;
using GameBackend.Services.Lobby.API.Domain;
using Microsoft.EntityFrameworkCore;

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
    /// При одновременных запросах на создание лобби для одного предмета уникальный индекс БД
    /// предотвратит дубликаты — проигравший запрос повторит lookup и вернёт уже созданное лобби.
    /// </summary>
    /// <param name="command">Команда создания лобби.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Идентификатор созданного лобби.</returns>
    public async Task<Guid> Handle(CreateLobbyCommand command, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetOpenLobbyByItemIdAsync(command.ItemId, cancellationToken);
        if (existing is not null)
            return existing.Id;

        var lobby = LobbyAggregate.Create(
            command.ItemId,
            command.ItemName,
            command.ItemImageUrl,
            command.ItemRarity,
            command.StartingPrice,
            command.MaxParticipants);

        try
        {
            await _repository.SaveAsync(lobby, cancellationToken);
            return lobby.Id;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            existing = await _repository.GetOpenLobbyByItemIdAsync(command.ItemId, cancellationToken);
            if (existing is not null)
                return existing.Id;

            throw;
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true
               || ex.InnerException?.Message.Contains("unique constraint", StringComparison.OrdinalIgnoreCase) == true;
    }
}
