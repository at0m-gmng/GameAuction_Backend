using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Catalog.API.Application.Items;

/// <summary>
/// DTO предмета из инвентаря игрока для отображения в профиле.
/// </summary>
/// <param name="ItemId">Идентификатор предмета каталога.</param>
/// <param name="Name">Название предмета.</param>
/// <param name="Description">Описание.</param>
/// <param name="Category">Категория.</param>
/// <param name="Rarity">Редкость.</param>
/// <param name="ImageUrl">Ссылка на изображение.</param>
/// <param name="StartingPrice">Базовая цена предмета в каталоге.</param>
/// <param name="Quantity">Количество копий в инвентаре.</param>
/// <param name="AcquiredAt">Дата первой покупки (UTC).</param>
public sealed record InventoryItemDto(
    Guid ItemId,
    string Name,
    string? Description,
    ItemCategory Category,
    ItemRarity Rarity,
    string? ImageUrl,
    decimal StartingPrice,
    int Quantity,
    DateTime AcquiredAt);