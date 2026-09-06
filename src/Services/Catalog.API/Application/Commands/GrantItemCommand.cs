using GameBackend.SharedKernel.Application;
using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Команда выдачи приватного предмета игроку (например, приветственного подарка).
/// Вызывается только другими сервисами backend'а. Возвращает идентификатор
/// созданной карточки предмета.
/// </summary>
/// <param name="PlayerId">Идентификатор игрока-получателя.</param>
/// <param name="Name">Название предмета.</param>
/// <param name="Description">Описание предмета.</param>
/// <param name="Category">Категория предмета.</param>
/// <param name="Rarity">Редкость предмета.</param>
/// <param name="ImageUrl">Ссылка на изображение.</param>
/// <param name="StartingPrice">Базовая цена предмета.</param>
public sealed record GrantItemCommand(
    Guid PlayerId,
    string Name,
    string? Description,
    ItemCategory Category,
    ItemRarity Rarity,
    string? ImageUrl,
    decimal StartingPrice) : ICommand<Guid>;
