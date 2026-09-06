using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Catalog.API.Application.Items;

/// <summary>
/// DTO каталожной карточки предмета для отображения на экране каталога.
/// </summary>
/// <param name="Id">Идентификатор предмета.</param>
/// <param name="Name">Название предмета.</param>
/// <param name="Description">Описание предмета.</param>
/// <param name="Category">Категория предмета.</param>
/// <param name="Rarity">Редкость предмета.</param>
/// <param name="ImageUrl">Ссылка на изображение предмета.</param>
/// <param name="StartingPrice">Начальная цена для торгов.</param>
/// <param name="Stock">Количество доступных предметов.</param>
public sealed record ItemCatalogDto(
    Guid Id,
    string Name,
    string? Description,
    ItemCategory Category,
    ItemRarity Rarity,
    string? ImageUrl,
    decimal StartingPrice,
    int Stock);