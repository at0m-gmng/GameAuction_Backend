using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Identity.API.Infrastructure.ExternalServices;

/// <summary>
/// Заготовка предмета, полученная от Generation.API.
/// </summary>
/// <param name="Name">Название предмета.</param>
/// <param name="Description">Описание предмета.</param>
/// <param name="Category">Категория предмета.</param>
/// <param name="Rarity">Редкость предмета.</param>
/// <param name="ImageUrl">Ссылка на изображение.</param>
/// <param name="StartingPrice">Итоговая цена после множителя редкости.</param>
public sealed record GeneratedItemResponse(
    string Name,
    string? Description,
    ItemCategory Category,
    ItemRarity Rarity,
    string? ImageUrl,
    decimal StartingPrice);
