using GameBackend.Services.Identity.API.Application.Commands;
using GameBackend.Services.Identity.API.Application.Queries;
using GameBackend.SharedKernel.Security;
using Microsoft.AspNetCore.Mvc;

namespace GameBackend.Services.Identity.API.Controllers;

/// <summary>Контракт запроса на списание средств с баланса игрока.</summary>
public sealed record DebitBalanceRequest(decimal Amount);

/// <summary>
/// Внутренние эндпоинты Identity.API. Принимают только сервисы с общим секретом.
/// </summary>
[ApiController]
[Route("api/auth/internal")]
public sealed class InternalController : ControllerBase
{
    private readonly DebitBalanceCommandHandler _debitBalance;
    private readonly GetPlayerBalanceQueryHandler _getBalance;
    private readonly IInternalCallerValidator _internalCallerValidator;

    /// <summary>
    /// Инициализирует контроллер хендлерами и валидатором внутренних вызовов.
    /// </summary>
    /// <param name="debitBalance">Хендлер списания баланса.</param>
    /// <param name="getBalance">Хендлер запроса баланса.</param>
    /// <param name="internalCallerValidator">Проверка X-Internal-Key.</param>
    public InternalController(
        DebitBalanceCommandHandler debitBalance,
        GetPlayerBalanceQueryHandler getBalance,
        IInternalCallerValidator internalCallerValidator)
    {
        _debitBalance = debitBalance;
        _getBalance = getBalance;
        _internalCallerValidator = internalCallerValidator;
    }

    /// <summary>
    /// Списывает средства с баланса игрока (например, победа в аукционе).
    /// </summary>
    /// <param name="playerId">Идентификатор игрока.</param>
    /// <param name="request">Сумма списания.</param>
    /// <param name="ct">Токен отмены.</param>
    [HttpPost("players/{playerId:guid}/debit")]
    public async Task<IActionResult> DebitBalance(Guid playerId, [FromBody] DebitBalanceRequest request, CancellationToken ct = default)
    {
        if (!_internalCallerValidator.IsValid(Request.Headers["X-Internal-Key"]))
            return Unauthorized();

        try
        {
            await _debitBalance.Handle(new DebitBalanceCommand(playerId, request.Amount), ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Возвращает текущий баланс игрока.
    /// </summary>
    /// <param name="playerId">Идентификатор игрока.</param>
    /// <param name="ct">Токен отмены.</param>
    [HttpGet("players/{playerId:guid}/balance")]
    public async Task<ActionResult<decimal>> GetBalance(Guid playerId, CancellationToken ct = default)
    {
        if (!_internalCallerValidator.IsValid(Request.Headers["X-Internal-Key"]))
            return Unauthorized();

        try
        {
            var balance = await _getBalance.Handle(new GetPlayerBalanceQuery(playerId), ct);
            return Ok(balance);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
