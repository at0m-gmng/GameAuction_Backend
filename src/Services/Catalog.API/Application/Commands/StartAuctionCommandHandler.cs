using GameBackend.Services.Catalog.API.Application.Interfaces;
using GameBackend.Services.Catalog.API.Infrastructure.ExternalServices;
using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Обработчик запуска аукциона: по выставленному предмету создаёт лобби или отдаёт уже открытое.
/// </summary>
public sealed class StartAuctionCommandHandler : ICommandHandler<StartAuctionCommand, Guid>
{
    private const int DefaultMaxParticipants = 5;

    private readonly IItemRepository _itemRepository;
    private readonly ILobbyServiceClient _lobbyClient;

    /// <summary>
    /// Инициализирует обработчик зависимостями.
    /// </summary>
    /// <param name="itemRepository">Репозиторий предметов каталога.</param>
    /// <param name="lobbyClient">Клиент к Lobby.API.</param>
    public StartAuctionCommandHandler(IItemRepository itemRepository, ILobbyServiceClient lobbyClient)
    {
        _itemRepository = itemRepository;
        _lobbyClient = lobbyClient;
    }

    /// <summary>
    /// Проверяет, что предмет выставлен, и возвращает идентификатор лобби (нового или уже открытого).
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Идентификатор лобби аукциона.</returns>
    public async Task<Guid> Handle(StartAuctionCommand command, CancellationToken cancellationToken)
    {
        var item = await _itemRepository.GetByIdAsync(command.ItemId, cancellationToken)
                   ?? throw new InvalidOperationException($"Предмет {command.ItemId} не найден в каталоге");

        if (!item.IsListed)
            throw new InvalidOperationException("Предмет не выставлен на продажу");

        return await _lobbyClient.CreateLobbyAsync(
            item.Id,
            item.Name,
            item.ImageUrl,
            item.Rarity,
            item.StartingPrice,
            DefaultMaxParticipants,
            cancellationToken);
    }
}
