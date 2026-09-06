using GameBackend.Services.Catalog.API.Application.Commands;
using GameBackend.Services.Catalog.API.Application.Items;
using GameBackend.Services.Catalog.API.Application.Queries;
using GameBackend.SharedKernel.Domain;
using Microsoft.AspNetCore.Mvc;

namespace GameBackend.Services.Catalog.API.Controllers;

/// <summary>
/// Контракт запроса на создание предмета.
/// </summary>
public sealed record CreateItemRequest(
    string Name,
    string? Description,
    ItemCategory Category,
    ItemRarity Rarity,
    string? ImageUrl,
    decimal StartingPrice,
    int Stock);

/// <summary>
/// Эндпоинты каталога предметов.
/// </summary>
[ApiController]
[Route("api/items")]
public sealed class ItemsController : ControllerBase
{
    private readonly CreateItemCommandHandler _create;
    private readonly GetItemsQueryHandler _get;

    /// <summary>
    /// Инициализирует контроллер обработчиками.
    /// </summary>
    /// <param name="create">Обработчик создания предмета.</param>
    /// <param name="get">Обработчик списка предметов.</param>
    public ItemsController(CreateItemCommandHandler create, GetItemsQueryHandler get)
    {
        _create = create;
        _get = get;
    }

    /// <summary>
    /// Возвращает список предметов каталога с фильтрацией и пагинацией.
    /// </summary>
    /// <param name="category">Фильтр по категории. Null — все.</param>
    /// <param name="minimumRarity">Фильтр по минимальной редкости. Null — любая.</param>
    /// <param name="page">Номер страницы (с 1).</param>
    /// <param name="pageSize">Размер страницы.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Список предметов.</returns>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ItemCatalogDto>>> GetItems(
        [FromQuery] ItemCategory? category,
        [FromQuery] ItemRarity? minimumRarity,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _get.Handle(new GetItemsQuery(category, minimumRarity, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>
    /// Создаёт новый предмет каталога.
    /// </summary>
    /// <param name="request">Данные нового предмета.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Идентификатор созданного предмета.</returns>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateItem([FromBody] CreateItemRequest request, CancellationToken ct = default)
    {
        var command = new CreateItemCommand(
            request.Name,
            request.Description,
            request.Category,
            request.Rarity,
            request.ImageUrl,
            request.StartingPrice,
            request.Stock);

        var id = await _create.Handle(command, ct);

        return Created($"/api/items/{id}", id);
    }
}