using GameBackend.Services.Catalog.API.Application.Commands;
using GameBackend.SharedKernel.Domain;
using GameBackend.SharedKernel.Security;
using Microsoft.AspNetCore.Mvc;

namespace GameBackend.Services.Catalog.API.Controllers;

/// <summary>
/// Контракт запроса на выдачу приватного предмета.
/// </summary>
public sealed record GrantItemRequest(
    Guid PlayerId,
    string Name,
    string? Description,
    ItemCategory Category,
    ItemRarity Rarity,
    string? ImageUrl,
    decimal StartingPrice);

/// <summary>Контракт запроса на передачу предмета победителю аукциона.</summary>
public sealed record AwardItemRequest(Guid PlayerId, int Quantity);

/// <summary>
/// Внутренние эндпоинты каталога. Принимают только сервисы с общим секретом.
/// </summary>
[ApiController]
[Route("api/catalog/internal")]
public sealed class InternalController : ControllerBase
{
    private readonly GrantItemCommandHandler _grantItem;
    private readonly BuyItemCommandHandler _buyItem;
    private readonly IInternalCallerValidator _internalCallerValidator;

    /// <summary>
    /// Инициализирует контроллер хендлерами и валидатором внутренних вызовов.
    /// </summary>
    /// <param name="grantItem">Хендлер выдачи предмета.</param>
    /// <param name="buyItem">Хендлер покупки/передачи предмета.</param>
    /// <param name="internalCallerValidator">Проверка X-Internal-Key.</param>
    public InternalController(
        GrantItemCommandHandler grantItem,
        BuyItemCommandHandler buyItem,
        IInternalCallerValidator internalCallerValidator)
    {
        _grantItem = grantItem;
        _buyItem = buyItem;
        _internalCallerValidator = internalCallerValidator;
    }

    /// <summary>
    /// Выдаёт игроку приватный предмет (например, приветственный подарок).
    /// </summary>
    /// <param name="request">Данные предмета и получателя.</param>
    /// <param name="ct">Токен отмены.</param>
    [HttpPost("grant-item")]
    public async Task<ActionResult<Guid>> GrantItem([FromBody] GrantItemRequest request, CancellationToken ct = default)
    {
        if (!_internalCallerValidator.IsValid(Request.Headers["X-Internal-Key"]))
            return Unauthorized();

        try
        {
            var command = new GrantItemCommand(
                request.PlayerId,
                request.Name,
                request.Description,
                request.Category,
                request.Rarity,
                request.ImageUrl,
                request.StartingPrice);

            var itemId = await _grantItem.Handle(command, ct);
            return Ok(itemId);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Списывает остаток публичного предмета и передаёт его победителю аукциона.
    /// </summary>
    /// <param name="itemId">Идентификатор предмета каталога.</param>
    /// <param name="request">Получатель и количество.</param>
    /// <param name="ct">Токен отмены.</param>
    [HttpPost("items/{itemId:guid}/award")]
    public async Task<IActionResult> AwardItem(Guid itemId, [FromBody] AwardItemRequest request, CancellationToken ct = default)
    {
        if (!_internalCallerValidator.IsValid(Request.Headers["X-Internal-Key"]))
            return Unauthorized();

        try
        {
            await _buyItem.Handle(new BuyItemCommand(request.PlayerId, itemId, request.Quantity), ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
