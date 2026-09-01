using GameBackend.Services.Catalog.API.Application.Common;
using GameBackend.Services.Catalog.API.Domain;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Команда создания новой каталожной карточки предмета.
/// Возвращает идентификатор созданного предмета.
/// </summary>
/// <param name="Name">Название предмета.</param>
/// <param name="Description">Описание предмета.</param>
/// <param name="Category">Категория предмета.</param>
/// <param name="Rarity">Редкость предмета.</param>
/// <param name="ImageUrl">Ссылка на изображение.</param>
/// <param name="StartingPrice">Начальная цена.</param>
/// <param name="Stock">Начальный остаток.</param>
public sealed record CreateItemCommand(
    string Name,
    string? Description,
    ItemCategory Category,
    ItemRarity Rarity,
    string? ImageUrl,
    decimal StartingPrice,
    int Stock) : ICommand<Guid>;