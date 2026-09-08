using GameBackend.Services.Catalog.API.Application.Commands;
using GameBackend.Services.Catalog.API.Application.Items;
using GameBackend.Services.Catalog.API.Application.Queries;
using GameBackend.SharedKernel.Application;
using GameBackend.SharedKernel.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace GameBackend.Services.Catalog.API.Controllers;

/// <summary>
/// Контракт запроса покупки.
/// </summary>
public sealed record BuyItemRequest(Guid ItemId, int Quantity = 1);

/// <summary>
/// Контракт запроса на выставление предмета из инвентаря на аукцион.
/// </summary>
public sealed record ListForAuctionRequest(decimal StartingPrice);

/// <summary>
/// Эндпоинты каталога: витрина (публично), инвентарь и покупка (по токену).
/// </summary>
[ApiController]
[Route("api/catalog")]
public sealed class CatalogController : ControllerBase
{
    private readonly GetItemsQueryHandler _getItems;
    private readonly GetInventoryQueryHandler _getInventory;
    private readonly BuyItemCommandHandler _buy;
    private readonly ListInventoryItemForAuctionCommandHandler _listForAuction;

    /// <summary>
    /// Инициализирует контроллер обработчиками.
    /// </summary>
    /// <param name="getItems">Запрос витрины.</param>
    /// <param name="getInventory">Запрос инвентаря.</param>
    /// <param name="buy">Команда покупки.</param>
    /// <param name="listForAuction">Команда выставления предмета на аукцион.</param>
    public CatalogController(
        GetItemsQueryHandler getItems,
        GetInventoryQueryHandler getInventory,
        BuyItemCommandHandler buy,
        ListInventoryItemForAuctionCommandHandler listForAuction)
    {
        _getItems = getItems;
        _getInventory = getInventory;
        _buy = buy;
        _listForAuction = listForAuction;
    }

    /// <summary>
    /// Публичная витрина предметов с фильтрацией и пагинацией.
    /// </summary>
    [HttpGet("items")]
    public async Task<ActionResult<IReadOnlyCollection<ItemCatalogDto>>> GetItems(
        [FromQuery] ItemCategory? category,
        [FromQuery] ItemRarity? minimumRarity,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _getItems.Handle(new GetItemsQuery(category, minimumRarity, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>
    /// Инвентарь текущего игрока. Требует JWT-токен.
    /// </summary>
    [Authorize]
    [HttpGet("inventory")]
    public async Task<ActionResult<IReadOnlyCollection<InventoryItemDto>>> GetInventory(CancellationToken ct = default)
    {
        var playerId = GetPlayerId();
        if (playerId is null) return Unauthorized();

        var result = await _getInventory.Handle(new GetInventoryQuery(playerId.Value), ct);
        return Ok(result);
    }

    /// <summary>
    /// Покупка предмета в инвентарь. Требует JWT-токен.
    /// </summary>
    [Authorize]
    [HttpPost("buy")]
    public async Task<IActionResult> BuyItem([FromBody] BuyItemRequest request, CancellationToken ct = default)
    {
        var playerId = GetPlayerId();
        if (playerId is null) return Unauthorized();

        try
        {
            await _buy.Handle(new BuyItemCommand(playerId.Value, request.ItemId, request.Quantity), ct);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Выставляет предмет из инвентаря на аукцион. Требует JWT-токен.
    /// </summary>
    [Authorize]
    [HttpPost("inventory/{itemId:guid}/list-for-auction")]
    public async Task<ActionResult<Guid>> ListForAuction(Guid itemId, [FromBody] ListForAuctionRequest request, CancellationToken ct = default)
    {
        var playerId = GetPlayerId();
        if (playerId is null) return Unauthorized();

        try
        {
            var lobbyId = await _listForAuction.Handle(
                new ListInventoryItemForAuctionCommand(playerId.Value, itemId, request.StartingPrice),
                ct);

            return Ok(lobbyId);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentOutOfRangeException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid? GetPlayerId()
    {
        var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                    ?? User.FindFirst("sub");

        if (claim is null || !Guid.TryParse(claim.Value, out var playerId))
            return null;

        return playerId;
    }
}