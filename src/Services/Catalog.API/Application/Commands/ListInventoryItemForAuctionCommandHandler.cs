using GameBackend.Services.Catalog.API.Application.Interfaces;
using GameBackend.Services.Catalog.API.Infrastructure.ExternalServices;
using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Обработчик команды выставления предмета из инвентаря на аукцион.
/// </summary>
public sealed class ListInventoryItemForAuctionCommandHandler : ICommandHandler<ListInventoryItemForAuctionCommand, Guid>
{
    private const int DefaultMaxParticipants = 5;

    private readonly IInventoryRepository _inventoryRepository;
    private readonly IItemRepository _itemRepository;
    private readonly ILobbyServiceClient _lobbyClient;

    /// <summary>
    /// Инициализирует обработчик зависимостями.
    /// </summary>
    /// <param name="inventoryRepository">Репозиторий инвентаря.</param>
    /// <param name="itemRepository">Репозиторий предметов каталога.</param>
    /// <param name="lobbyClient">Клиент к Lobby.API.</param>
    public ListInventoryItemForAuctionCommandHandler(
        IInventoryRepository inventoryRepository,
        IItemRepository itemRepository,
        ILobbyServiceClient lobbyClient)
    {
        _inventoryRepository = inventoryRepository;
        _itemRepository = itemRepository;
        _lobbyClient = lobbyClient;
    }

    /// <summary>
    /// Создаёт лобби для предмета и только затем списывает его из инвентаря игрока.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Идентификатор созданного лобби.</returns>
    public async Task<Guid> Handle(ListInventoryItemForAuctionCommand command, CancellationToken cancellationToken)
    {
        if (command.StartingPrice <= 0)
            throw new ArgumentOutOfRangeException(nameof(command.StartingPrice), "Стартовая цена должна быть больше нуля");

        var inventoryItem = await _inventoryRepository.GetByPlayerAndItemAsync(command.PlayerId, command.ItemId, cancellationToken);
        if (inventoryItem is null || inventoryItem.Quantity < 1)
            throw new InvalidOperationException("Предмет не найден в инвентаре");

        var item = await _itemRepository.GetByIdAsync(command.ItemId, cancellationToken)
                   ?? throw new InvalidOperationException($"Предмет {command.ItemId} не найден в каталоге");

        // NOTE: сначала лобби, инвентарь — только при успехе; иначе при сбое Lobby.API игрок теряет вещь без аукциона.
        var lobbyId = await _lobbyClient.CreateLobbyAsync(
            item.Id,
            item.Name,
            item.ImageUrl,
            command.StartingPrice,
            DefaultMaxParticipants,
            cancellationToken);

        inventoryItem.RemoveQuantity(1);

        if (inventoryItem.Quantity == 0)
            await _inventoryRepository.DeleteAsync(inventoryItem, cancellationToken);
        else
            await _inventoryRepository.SaveAsync(inventoryItem, cancellationToken);

        return lobbyId;
    }
}
