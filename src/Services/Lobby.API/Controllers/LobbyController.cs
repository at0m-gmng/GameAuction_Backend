using GameBackend.Services.Lobby.API.Application.Commands;
using GameBackend.Services.Lobby.API.Application.Lobbies;
using GameBackend.Services.Lobby.API.Application.Queries;
using GameBackend.SharedKernel.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace GameBackend.Services.Lobby.API.Controllers;

/// <summary>Контракт запроса на создание лобби.</summary>
public sealed record CreateLobbyRequest(
    Guid ItemId,
    string ItemName,
    string? ItemImageUrl,
    decimal StartingPrice,
    int MaxParticipants);

/// <summary>Контракт запроса на ставку.</summary>
public sealed record PlaceBidRequest(decimal Amount);

/// <summary>
/// Эндпоинты лобби: список, детали, создание, присоединение, ставки; старт и завершение — автоматические.
/// </summary>
[ApiController]
[Route("api/lobbies")]
public sealed class LobbyController : ControllerBase
{
    private readonly CreateLobbyCommandHandler _create;
    private readonly JoinLobbyCommandHandler _join;
    private readonly PlaceBidCommandHandler _placeBid;
    private readonly GetOpenLobbiesQueryHandler _openLobbies;
    private readonly GetLobbyQueryHandler _getLobby;
    private readonly IInternalCallerValidator _internalCallerValidator;

    /// <summary>
    /// Инициализирует контроллер обработчиками.
    /// </summary>
    public LobbyController(
        CreateLobbyCommandHandler create,
        JoinLobbyCommandHandler join,
        PlaceBidCommandHandler placeBid,
        GetOpenLobbiesQueryHandler openLobbies,
        GetLobbyQueryHandler getLobby,
        IInternalCallerValidator internalCallerValidator)
    {
        _create = create;
        _join = join;
        _placeBid = placeBid;
        _openLobbies = openLobbies;
        _getLobby = getLobby;
        _internalCallerValidator = internalCallerValidator;
    }

    /// <summary>
    /// Возвращает список открытых лобби (Gathering и Bidding).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<LobbyListDto>>> GetOpenLobbies(CancellationToken ct = default)
    {
        var result = await _openLobbies.Handle(new GetOpenLobbiesQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Возвращает детальную информацию лобби для экрана лобби.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LobbyDetailsDto>> GetLobby(Guid id, CancellationToken ct = default)
    {
        var result = await _getLobby.Handle(new GetLobbyQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Создаёт новое лобби для предмета; вызывается другими сервисами, не игроками — требует X-Internal-Key.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateLobby([FromBody] CreateLobbyRequest request, CancellationToken ct = default)
    {
        if (!_internalCallerValidator.IsValid(Request.Headers["X-Internal-Key"]))
            return Unauthorized();

        try
        {
            var id = await _create.Handle(
                new CreateLobbyCommand(request.ItemId, request.ItemName, request.ItemImageUrl, request.StartingPrice, request.MaxParticipants),
                ct);

            return Created($"/api/lobbies/{id}", id);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentOutOfRangeException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Присоединяет вызывающего игрока к лобби. Требует JWT-токен.
    /// </summary>
    [Authorize]
    [HttpPost("{id:guid}/join")]
    public async Task<IActionResult> Join(Guid id, CancellationToken ct = default)
    {
        var playerId = GetPlayerId();
        if (playerId is null) return Unauthorized();

        try
        {
            await _join.Handle(new JoinLobbyCommand(id, playerId.Value), ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Регистрирует ставку вызывающего игрока. Требует JWT-токен.
    /// </summary>
    [Authorize]
    [HttpPost("{id:guid}/bids")]
    public async Task<IActionResult> PlaceBid(Guid id, [FromBody] PlaceBidRequest request, CancellationToken ct = default)
    {
        var playerId = GetPlayerId();
        if (playerId is null) return Unauthorized();

        try
        {
            await _placeBid.Handle(new PlaceBidCommand(id, playerId.Value, request.Amount), ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid? GetPlayerId()
    {
        var claim = User.FindFirst(JwtRegisteredClaimNames.Sub);

        if (claim is null || !Guid.TryParse(claim.Value, out var playerId))
            return null;

        return playerId;
    }
}