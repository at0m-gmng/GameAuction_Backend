using GameBackend.Services.Lobby.API.Application.Commands;
using GameBackend.Services.Lobby.API.Application.Lobbies;
using GameBackend.Services.Lobby.API.Application.Queries;
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

/// <summary>Контракт запроса на запуск аукциона.</summary>
public sealed record StartAuctionRequest(int DurationSeconds);

/// <summary>
/// Эндпоинты лобби: список, детали, создание, присоединение, ставки, жизненный цикл аукциона.
/// </summary>
[ApiController]
[Route("api/lobbies")]
public sealed class LobbyController : ControllerBase
{
    private readonly CreateLobbyCommandHandler _create;
    private readonly JoinLobbyCommandHandler _join;
    private readonly StartAuctionCommandHandler _start;
    private readonly PlaceBidCommandHandler _placeBid;
    private readonly CompleteAuctionCommandHandler _complete;
    private readonly GetOpenLobbiesQueryHandler _openLobbies;
    private readonly GetLobbyQueryHandler _getLobby;

    /// <summary>
    /// Инициализирует контроллер обработчиками.
    /// </summary>
    public LobbyController(
        CreateLobbyCommandHandler create,
        JoinLobbyCommandHandler join,
        StartAuctionCommandHandler start,
        PlaceBidCommandHandler placeBid,
        CompleteAuctionCommandHandler complete,
        GetOpenLobbiesQueryHandler openLobbies,
        GetLobbyQueryHandler getLobby)
    {
        _create = create;
        _join = join;
        _start = start;
        _placeBid = placeBid;
        _complete = complete;
        _openLobbies = openLobbies;
        _getLobby = getLobby;
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
    /// Создаёт новое лобби для предмета.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateLobby([FromBody] CreateLobbyRequest request, CancellationToken ct = default)
    {
        var id = await _create.Handle(
            new CreateLobbyCommand(request.ItemId, request.ItemName, request.ItemImageUrl, request.StartingPrice, request.MaxParticipants),
            ct);

        return Created($"/api/lobbies/{id}", id);
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

        await _join.Handle(new JoinLobbyCommand(id, playerId.Value), ct);
        return NoContent();
    }

    /// <summary>
    /// Запускает аукцион в лобби.
    /// </summary>
    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id, [FromBody] StartAuctionRequest request, CancellationToken ct = default)
    {
        await _start.Handle(new StartAuctionCommand(id, TimeSpan.FromSeconds(request.DurationSeconds)), ct);
        return NoContent();
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

        await _placeBid.Handle(new PlaceBidCommand(id, playerId.Value, request.Amount), ct);
        return NoContent();
    }

    /// <summary>
    /// Завершает аукцион и определяет победителя.
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct = default)
    {
        await _complete.Handle(new CompleteAuctionCommand(id), ct);
        return NoContent();
    }

    private Guid? GetPlayerId()
    {
        var claim = User.FindFirst(JwtRegisteredClaimNames.Sub);

        if (claim is null || !Guid.TryParse(claim.Value, out var playerId))
            return null;

        return playerId;
    }
}