using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Identity.API.Infrastructure.ExternalServices;

// NOTE: форма записи должна совпадать с Catalog.API.Controllers.GrantItemRequest —
// сервисы не делятся общей сборкой контрактов, синхронизация ручная.

/// <summary>
/// Запрос на выдачу приватного предмета в Catalog.API.
/// </summary>
/// <param name="PlayerId">Идентификатор игрока-получателя.</param>
/// <param name="Name">Название предмета.</param>
/// <param name="Description">Описание предмета.</param>
/// <param name="Category">Категория предмета.</param>
/// <param name="Rarity">Редкость предмета.</param>
/// <param name="ImageUrl">Ссылка на изображение.</param>
/// <param name="StartingPrice">Базовая цена предмета.</param>
public sealed record GrantItemRequest(
    Guid PlayerId,
    string Name,
    string? Description,
    ItemCategory Category,
    ItemRarity Rarity,
    string? ImageUrl,
    decimal StartingPrice);
