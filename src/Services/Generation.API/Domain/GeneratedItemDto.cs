using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Generation.API.Domain;

/// <summary>
/// Заготовка предмета, которую Generation отдаёт вызывающему сервису.
/// </summary>
/// <param name="Name">Имя предмета.</param>
/// <param name="Description">Описание предмета.</param>
/// <param name="Category">Категория предмета.</param>
/// <param name="Rarity">Редкость предмета.</param>
/// <param name="ImageUrl">URL превью предмета.</param>
/// <param name="StartingPrice">Итоговая цена после множителя.</param>
public sealed record GeneratedItemDto(
    string Name,
    string? Description,
    ItemCategory Category,
    ItemRarity Rarity,
    string? ImageUrl,
    decimal StartingPrice);